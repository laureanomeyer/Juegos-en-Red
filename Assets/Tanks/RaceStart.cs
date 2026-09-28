using Photon.Pun;
using System;
using UnityEngine;

public class RaceStart : MonoBehaviourPun
{
    private PhotonView myView;
    private DateTime? startTime = null;
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

        if (DateTime.UtcNow >= startTime.Value.ToUniversalTime() && raceStarted == false)
        {
            movement.SetMove(true);
            raceStarted = true;
            Debug.Log("Carrera empezada");
        }
    }

    public void OnJump()
    {
        if (!myView.IsMine) return;
        if (!PhotonNetwork.IsMasterClient) return;
        Debug.Log("Start apretado");
        if (!triggered)
        {
            triggered = true;
            photonView.RPC(nameof(RPC_SetStartTime), RpcTarget.AllBufferedViaServer);
        }
    }

    [PunRPC]
    private void RPC_SetStartTime()
    {
        Debug.Log("Start seteado");
        var timeToStart = DateTime.UtcNow.AddSeconds(5f);
        startTime = timeToStart;
    }
}
