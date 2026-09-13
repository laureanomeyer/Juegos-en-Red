using Photon.Pun;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint; 

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();
        if (movement == null) return;

        PhotonView view = movement.GetComponent<PhotonView>();
        if (view == null || !view.IsMine) return; 

        movement.SetCheckpoint(respawnPoint.position);
    }
}
