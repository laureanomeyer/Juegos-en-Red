using UnityEngine;
using Photon.Pun;

public class MapSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject map;
    [SerializeField] private GameObject props;
    [SerializeField] private int objectsCount;

    private void Awake()
    {
        PhotonManager.Instance.OnRoom += SpawnMap;
    }

    private void SpawnMap()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.InstantiateRoomObject(map.name, map.transform.position, map.transform.rotation);
        CreateObjects();
    }

    private void CreateObjects()
    {
        for (int i = 0; i <= objectsCount; i++)
        {
            var go = PhotonNetwork.InstantiateRoomObject(props.name, new Vector3(Random.Range(-15, 5), Random.Range(0.5f, 5), Random.Range(-15f, 5)), props.transform.rotation);
            go.SetActive(true);
        }
    }
}
