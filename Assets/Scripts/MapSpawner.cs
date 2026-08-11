using UnityEngine;
using Photon.Pun;

public class MapSpawner : MonoBehaviour
{
    [SerializeField] private GameObject map;

    private void Awake()
    {
        PhotonManager.Instance.OnRoom += SpawnMap;
    }

    private void SpawnMap()
    {
        PhotonNetwork.InstantiateRoomObject(map.name, map.transform.position, map.transform.rotation);
    }
}
