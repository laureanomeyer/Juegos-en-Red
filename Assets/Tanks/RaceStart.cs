using Photon.Pun;
using System;

public class RaceStart : MonoBehaviourPun
{
    private PhotonView myView;
    private DateTime startTime;
    private bool triggered = false;
    private bool raceStarted = false;
    private TankMovement movement;

    private void Awake()
    {
        myView = GetComponent<PhotonView>();
        movement = GetComponent<TankMovement>();
    }

    private void Update()
    {
        if (!myView.IsMine) return;
        if (startTime == null) return; 

        if (DateTime.UtcNow >= startTime)
        {
            movement.SetMove(true);
        }
    }

    public void OnJump()
    {
        if (!myView.IsMine) return;
        if (!PhotonNetwork.IsMasterClient) return;

        if (!triggered)
        {
            triggered = true;
            photonView.RPC(nameof(RPC_SetStartTime), RpcTarget.AllBufferedViaServer);
        }
    }

    [PunRPC]
    private void RPC_SetStartTime()
    {
        var timeToStart = DateTime.UtcNow.AddSeconds(5f);
        startTime = timeToStart;
    }
}
