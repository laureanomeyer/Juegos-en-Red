using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class ZoneLevelBuilder : MonoBehaviourPun
{
    [SerializeField] private GameObject[] zonePrefabs;
    [SerializeField] private Vector3 origen = Vector3.zero;
    [SerializeField] private float trapCooldown = 5f;
    [SerializeField] private GameObject safeRoomPrefab;
    [SerializeField] private int insertarDespuesDeZona = 5;
    [SerializeField] private GameObject finishLinePrefab;
    [SerializeField] private GameObject startPrefab;
    [SerializeField] private float rotacionGlobalY = 0f;

    private readonly List<ITrap> trampas = new List<ITrap>();
    private float[] nextAvailableTime;

    private void Start()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PhotonManager.ZONE_SEQ_KEY, out object raw))
        {
            return;
        }

        ConstruirNivel((int[])raw);
    }

    private void ConstruirNivel(int[] secuencia)
    {
        Vector3 cursor = origen;

        if (startPrefab != null)
        {
            cursor = InstanciarPieza(startPrefab, cursor, out _);
        }

        bool checkpointInsertado = false;

        for (int i = 0; i < secuencia.Length; i++)
        {
            if (!checkpointInsertado && safeRoomPrefab != null && i == insertarDespuesDeZona)
            {
                cursor = InstanciarPieza(safeRoomPrefab, cursor, out _);
                checkpointInsertado = true;
            }

            int index = secuencia[i];
            cursor = InstanciarPieza(zonePrefabs[index], cursor, out GameObject zonaGO);
            var botones = zonaGO.GetComponentsInChildren<ButtonBehavior>(true);

            foreach (var boton in botones)
            {
                ITrap trapDelBoton = boton.Trap;
                if (trapDelBoton == null)
                {
                    continue;
                }

                trampas.Add(trapDelBoton);
                boton.Initialize(this, trampas.Count - 1);
            }
        }

        if (finishLinePrefab != null)
        {
            InstanciarPieza(finishLinePrefab, cursor, out _);
        }

        nextAvailableTime = new float[trampas.Count];
    }

    private Vector3 InstanciarPieza(GameObject prefab, Vector3 cursorPos, out GameObject instancia)
    {
        Quaternion offset = Quaternion.Euler(0f, rotacionGlobalY, 0f);
        Quaternion rot = offset * prefab.transform.rotation;
        RoomEndpoints endpoints = prefab.GetComponentInChildren<RoomEndpoints>();

        if (endpoints == null)
        {
            instancia = Instantiate(prefab, cursorPos, rot, transform);
            return cursorPos;
        }

        Vector3 pos = cursorPos - rot * endpoints.HeadLocalPos;
        instancia = Instantiate(prefab, pos, rot, transform);

        return pos + rot * endpoints.TailLocalPos;
    }

    public bool CanTriggerTrap(int zoneIndex)
    {
        bool result = zoneIndex >= 0 && zoneIndex < trampas.Count && Time.time >= nextAvailableTime[zoneIndex];
        return result;
    }

    public void RequestTriggerTrap(int zoneIndex)
    {
        if (!PhotonManager.Instance.IsLocalPlayerTrapMaster())
        {
            return;
        }

        if (!CanTriggerTrap(zoneIndex)) return;

        nextAvailableTime[zoneIndex] = Time.time + trapCooldown;
        photonView.RPC(nameof(RPC_TriggerTrap), RpcTarget.All, zoneIndex);
    }

    [PunRPC]
    private void RPC_TriggerTrap(int zoneIndex, PhotonMessageInfo info)
    {
        int trapMasterActor = PhotonManager.Instance.GetTrapMasterActor();

        if (info.Sender == null || info.Sender.ActorNumber != trapMasterActor)
        {
            return;
        }

        if (zoneIndex < 0 || zoneIndex >= trampas.Count) return;
        trampas[zoneIndex]?.Activate();
    }
}