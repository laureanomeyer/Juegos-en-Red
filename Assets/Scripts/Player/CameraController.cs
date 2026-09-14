using Photon.Pun;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private PhotonView view;

    private bool isMaster;

    private void Awake()
    {
        isMaster = PhotonManager.Instance.IsLocalPlayerTrapMaster();
    }

    private void Update()
    {
        if (!view.IsMine) return;
        HandleCameras(isMaster);
    }

    private void HandleCameras(bool isMain)
    {
        if (!view.IsMine) return;
        if (target == null) return;

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
