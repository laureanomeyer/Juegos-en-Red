using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Rol")]
    [Tooltip("Tildado: esta instancia spawnea al Master (controla trampas). Destildado: spawnea a los Corredores.")]
    [SerializeField] private bool spawnForMaster;

    [Header("Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnSpread = 6f;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[PlayerSpawner] No estamos en una sala, no se instancia el jugador.");
            return;
        }

        bool isMaster = PhotonNetwork.IsMasterClient;
        if (isMaster != spawnForMaster) return; // este spawner no es responsable de este cliente

        Vector3 basePos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Vector3 spawnPos = new Vector3(
            basePos.x + Random.Range(-spawnSpread, spawnSpread),
            basePos.y,
            basePos.z + Random.Range(-spawnSpread, spawnSpread)
        );

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, playerPrefab.transform.rotation);
    }
}