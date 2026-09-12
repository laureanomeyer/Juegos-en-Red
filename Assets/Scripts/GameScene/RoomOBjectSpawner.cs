using UnityEngine;
using Photon.Pun;

public class RoomObjectSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject mainObject;
    [SerializeField] private GameObject secondaryObject;
    [SerializeField] private int secondaryObjectsCount;

    private void Awake()
    {
        PhotonManager.Instance.OnRoom += SpawnRoomObjects;
    }

    private void SpawnRoomObjects()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.InstantiateRoomObject(mainObject.name, mainObject.transform.position, mainObject.transform.rotation);
        SpawnSecondaryObjects();
    }

    private void SpawnSecondaryObjects()
    {
        for (int i = 0; i < secondaryObjectsCount; i++)
        {
            var go = PhotonNetwork.InstantiateRoomObject(secondaryObject.name, new Vector3(Random.Range(-15, 5), Random.Range(0.5f, 5), Random.Range(-15f, 5)), secondaryObject.transform.rotation);
            go.SetActive(true);
        }
    }
}
