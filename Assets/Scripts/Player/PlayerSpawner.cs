using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        Debug.Log($"[PlayerSpawner] Awake. PhotonManager.Instance es null? {PhotonManager.Instance == null}");
        PhotonManager.Instance.OnRoom += SpawnPlayer;
        Debug.Log("[PlayerSpawner] Suscripto a OnRoom");
    }

    private void SpawnPlayer()
    {
        Debug.Log("[PlayerSpawner] SpawnPlayer() llamado");
        var go = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(playerPrefab.transform.position.x + Random.Range(0, 18), playerPrefab.transform.position.y, playerPrefab.transform.position.z), playerPrefab.transform.rotation);
        Debug.Log($"[PlayerSpawner] Instantiate devolvió: {(go == null ? "NULL" : go.name)}");
    }
}
