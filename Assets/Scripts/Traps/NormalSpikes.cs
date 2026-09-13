using Photon.Pun;
using UnityEngine;

public class NormalSpikes : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerVitals vitals = other.GetComponentInParent<PlayerVitals>();
        if (vitals == null) return;

        if (!vitals.photonView.IsMine) return;

        vitals.photonView.RPC(nameof(PlayerVitals.LoseLife), RpcTarget.All);
    }
}
