using Photon.Pun;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;

    private bool isMaster;

    private void Awake()
    {
        isMaster = PhotonNetwork.IsMasterClient;
    }

    private void Update()
    {
        HandleCameras(isMaster);
    }

    private void HandleCameras(bool isMain)
    {
        if (isMain) 
        {
            Vector3 targetPos = new Vector3(target.position.x, transform.position.y, transform.position.z);
            transform.position = targetPos;
        }
        else
        {
            Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.position = targetPos;
        }
    }

}
