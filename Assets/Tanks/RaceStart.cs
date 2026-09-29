using Photon.Pun;
using UnityEngine;

public class RaceStart : MonoBehaviourPun
{
    private double startTime = -1;
    private bool triggered = false;
    private bool raceStarted = false;

    private TankMovement movement;

    private void Awake()
    {
        movement = GetComponent<TankMovement>();
    }

    private void Update()
    {
        if (raceStarted) return;
        if (startTime < 0) return;

        if (PhotonNetwork.Time >= startTime)
        {
            raceStarted = true;
            movement.SetMove(true);
            Debug.Log("Carrera empezada");
        }
    }

    public void OnJump()
    {
        if (!PhotonNetwork.IsMasterClient || triggered) return;

        triggered = true;

        double t = PhotonNetwork.Time + 5.0f;
        photonView.RPC(nameof(RPC_SetStartTime), RpcTarget.AllBufferedViaServer, t);
    }

    [PunRPC]
    private void RPC_SetStartTime(double t)
    {
        foreach (var race in FindObjectsByType<RaceStart>(FindObjectsSortMode.None))
            race.startTime = t;
    }
}