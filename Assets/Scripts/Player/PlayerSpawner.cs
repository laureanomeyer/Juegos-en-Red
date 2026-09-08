using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        Debug.Log($"[PlayerSpawner] Awake. PhotonManager.Instance es null? {PhotonManager.Instance == null}");
       // PhotonManager.Instance.OnRoom += SpawnPlayer;
        Debug.Log("[PlayerSpawner] Suscripto a OnRoom");
    }

    private void Start()
    {
  
        if (PhotonNetwork.InRoom)
        {
            Vector3 spawnPos = new Vector3(
                playerPrefab.transform.position.x + Random.Range(0, 10),
                playerPrefab.transform.position.y,
                playerPrefab.transform.position.z
            );
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, playerPrefab.transform.rotation);
        }
    }
}
