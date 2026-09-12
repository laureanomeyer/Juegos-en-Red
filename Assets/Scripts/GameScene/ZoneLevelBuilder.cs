using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Vive en la GameScene. Lee la secuencia de zonas que el Master generó
// y guardó como Custom Property de la sala al crearla (PhotonManager.CreateRoom),
// e instancia los prefabs LOCALMENTE en cada cliente. No hace falta que sea
// networked: como todos leen la misma property, todos arman el mismo nivel.
[RequireComponent(typeof(PhotonView))]
public class ZoneLevelBuilder : MonoBehaviourPun
{
    [Tooltip("Mismo orden y misma cantidad de elementos en TODOS los clientes.")]
    [SerializeField] private GameObject[] zonePrefabs;
    [SerializeField] private float zonaLength = 50f;
    [SerializeField] private Vector3 origen = Vector3.zero;
    [SerializeField] private Vector3 direccion = Vector3.forward;
    [SerializeField] private float trapCooldown = 5f;

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
        Vector3 dirNorm = direccion.normalized;

        for (int i = 0; i < secuencia.Length; i++)
        {
            int index = secuencia[i];
            Vector3 pos = origen + dirNorm * (zonaLength * i);
            GameObject zonaGO = Instantiate(zonePrefabs[index], pos, Quaternion.identity, transform);

            var trapComponent = zonaGO.GetComponentInChildren<ITrap>();
            trampas.Add(trapComponent);

            var boton = zonaGO.GetComponentInChildren<ButtonBehavior>();
            boton?.Initialize(this, i); // el botón de ESTA copia queda atado a SU índice
        }

        nextAvailableTime = new float[trampas.Count];
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