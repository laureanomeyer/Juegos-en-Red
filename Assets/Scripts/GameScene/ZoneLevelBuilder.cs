using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Vive en la GameScene. Lee la secuencia de zonas que el Master generó
// y guardó como Custom Property de la sala al crearla (PhotonManager.CreateRoom),
// e instancia los prefabs LOCALMENTE en cada cliente. No hace falta que sea
// networked: como todos leen la misma property, todos arman el mismo nivel.
// Cada pieza (zona, safe room, finish line) se encadena por sus puntos
// Head/Tail (ver RoomEndpoints), así que el tamaño real de cada prefab
// no importa: la distancia y orientación las define la geometría, no un
// valor fijo en el Inspector.
[RequireComponent(typeof(PhotonView))]
public class ZoneLevelBuilder : MonoBehaviourPun
{
    [Tooltip("Mismo orden y misma cantidad de elementos en TODOS los clientes.")]
    [SerializeField] private GameObject[] zonePrefabs;
    [SerializeField] private Vector3 origen = Vector3.zero;
    [SerializeField] private float trapCooldown = 5f;

    [Header("Checkpoint / Habitación Segura")]
    [SerializeField] private GameObject safeRoomPrefab;
    [Tooltip("Después de completar N zonas de trampa se inserta la habitación segura. Ej: 5 = después de la 5ta trampa.")]
    [SerializeField] private int insertarDespuesDeZona = 5;

    [Header("Meta")]
    [SerializeField] private GameObject finishLinePrefab;

    [Header("Inicio")]
    [SerializeField] private GameObject startPrefab;

    [Header("Orientación")]
    [Tooltip("180 invierte la dirección completa del recorrido sin tocar los prefabs individuales.")]
    [SerializeField] private float rotacionGlobalY = 0f;

    private readonly List<ITrap> trampas = new List<ITrap>();
    private float[] nextAvailableTime;

    private void Start()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PhotonManager.ZONE_SEQ_KEY, out object raw))
        {
            Debug.LogError("No se encontró la secuencia de zonas en las Custom Properties de la sala.");
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

            var trapComponent = zonaGO.GetComponentInChildren<ITrap>();
            trampas.Add(trapComponent);

            var boton = zonaGO.GetComponentInChildren<ButtonBehavior>();
            boton?.Initialize(this, trampas.Count - 1);
        }

        if (finishLinePrefab != null)
        {
            InstanciarPieza(finishLinePrefab, cursor, out _);
        }

        nextAvailableTime = new float[trampas.Count];
    }

    // Instancia 'prefab' de forma que su Head caiga en 'cursorPos', y devuelve
    // la posición mundial de su Tail (el cursor para la próxima pieza).
    private Vector3 InstanciarPieza(GameObject prefab, Vector3 cursorPos, out GameObject instancia)
    {
        Quaternion offset = Quaternion.Euler(0f, rotacionGlobalY, 0f);
        Quaternion rot = offset * prefab.transform.rotation;
        RoomEndpoints endpoints = prefab.GetComponentInChildren<RoomEndpoints>();

        if (endpoints == null)
        {
            Debug.LogError($"'{prefab.name}' no tiene RoomEndpoints (Head/Tail) asignado.");
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
        Debug.Log($"[Builder] CanTrigger check: zoneIndex={zoneIndex}, trampas.Count={trampas.Count}, nextAvailableTime.Length={(nextAvailableTime?.Length ?? -1)}");
        return result;
    }

    public void RequestTriggerTrap(int zoneIndex)
    {
        Debug.Log($"[Builder] RequestTriggerTrap zone={zoneIndex}, CanTrigger={CanTriggerTrap(zoneIndex)}");
        if (!CanTriggerTrap(zoneIndex)) return;

        nextAvailableTime[zoneIndex] = Time.time + trapCooldown;
        photonView.RPC(nameof(RPC_TriggerTrap), RpcTarget.All, zoneIndex);
    }

    [PunRPC]
    private void RPC_TriggerTrap(int zoneIndex)
    {
        Debug.Log($"[Builder] RPC recibido, zone={zoneIndex}, trampa null? {trampas[zoneIndex] == null}");
        if (zoneIndex < 0 || zoneIndex >= trampas.Count) return;
        trampas[zoneIndex]?.Activate();
    }
}