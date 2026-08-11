using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void Awake()
    {
        PhotonManager.Instance.OnRoom += SpawnPlayer;
    }

    private void SpawnPlayer()
    {
        PhotonNetwork.Instantiate(player.name, new Vector3 (player.transform.position.x + Random.Range(0, 18), player.transform.position.y, player.transform.position.z), player.transform.rotation);
    }
}
