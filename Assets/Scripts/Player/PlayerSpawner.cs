using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
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

        Vector3 basePos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Vector3 spawnPos = new Vector3(
            basePos.x + Random.Range(-spawnSpread, spawnSpread),
            basePos.y,
            basePos.z + Random.Range(-spawnSpread, spawnSpread)
        );

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, playerPrefab.transform.rotation);
    }
}