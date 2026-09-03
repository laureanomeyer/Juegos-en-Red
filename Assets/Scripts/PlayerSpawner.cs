using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        PhotonManager.Instance.OnRoom += SpawnPlayer;
    }

    private void SpawnPlayer()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3 (playerPrefab.transform.position.x + Random.Range(0, 18), playerPrefab.transform.position.y, playerPrefab.transform.position.z), playerPrefab.transform.rotation);
    }
}
