using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-90)]
[RequireComponent(typeof(PhotonView))]
public class RaceManager : MonoBehaviourPun
{
    public static RaceManager Instance;

    public Action<Player> OnRunnerFinished;
    public Action OnRunnersWon;   // al menos uno llegó
    public Action OnRunnersLost;  // todos los corredores perdieron sus 4 vidas
    public Action OnMasterWon;    // simétrico de OnRunnersLost, para la UI del master

    private readonly HashSet<int> finishedActors = new HashSet<int>();
    private readonly HashSet<int> eliminatedActors = new HashSet<int>();

    private void Awake() => Instance = this;

    public void ReportFinish()
    {
        photonView.RPC(nameof(RPC_ReportFinish), RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    private void RPC_ReportFinish(int actorNumber)
    {
        if (!finishedActors.Add(actorNumber)) return;
        OnRunnerFinished?.Invoke(PhotonNetwork.CurrentRoom.Players[actorNumber]);
        OnRunnersWon?.Invoke();
    }

    public void ReportEliminated(int actorNumber)
    {
        if (!eliminatedActors.Add(actorNumber)) return;
        CheckAllRunnersEliminated();
    }

    private void CheckAllRunnersEliminated()
    {
        int totalRunners = PhotonNetwork.CurrentRoom.PlayerCount - 1; // todos menos el trap master
        if (eliminatedActors.Count >= totalRunners)
        {
            OnRunnersLost?.Invoke();
            OnMasterWon?.Invoke();
        }
    }
}
