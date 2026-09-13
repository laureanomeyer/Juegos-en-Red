using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Rol")]
    [Tooltip("Tildado: esta instancia spawnea al Master (controla trampas). Destildado: spawnea a los Corredores.")]
    [SerializeField] private bool spawnForMaster;

    [Header("Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint; // Empty hijo de ESTE mismo prefab (StartRoom), dentro de la spawn zone
    [SerializeField] private float spawnSpread = 6f;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[PlayerSpawner] No estamos en una sala, no se instancia el jugador.");
            return;
        }

        bool isMaster = PhotonManager.Instance.IsLocalPlayerTrapMaster();

        Debug.Log($"[Spawner] spawnForMaster={spawnForMaster} | LocalActor={PhotonNetwork.LocalPlayer.ActorNumber} | TrapMasterActor={PhotonManager.Instance.GetTrapMasterActor()} | isMaster={isMaster} | ¿dispara este spawner?={isMaster == spawnForMaster}");

        if (isMaster != spawnForMaster) return;

        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;
        Vector3 spawnPos = new Vector3(
            basePos.x + Random.Range(-spawnSpread, spawnSpread),
            basePos.y,
            basePos.z + Random.Range(-spawnSpread, spawnSpread)
        );

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, playerPrefab.transform.rotation);

        Debug.Log($"[Spawner] Sala: {PhotonNetwork.CurrentRoom.Name} | Tiene ZONE_SEQ_KEY? {PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(PhotonManager.ZONE_SEQ_KEY)} | Tiene TRAP_MASTER_ACTOR_KEY? {PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(PhotonManager.TRAP_MASTER_ACTOR_KEY)}");
    }
}