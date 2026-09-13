using Photon.Pun;
using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PhotonView view = other.GetComponentInParent<PhotonView>();
        if (view == null || !view.IsMine) return;
        if (view.GetComponent<PlayerVitals>() == null) return; // solo jugadores, no props

        RaceManager.Instance.ReportFinish();
    }
}
