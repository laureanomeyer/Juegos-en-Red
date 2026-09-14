using Photon.Pun;
using System;
using UnityEngine;

[DefaultExecutionOrder(-90)]
[RequireComponent(typeof(PhotonView))]
public class RaceManager : MonoBehaviourPun
{
    public static RaceManager Instance;

    [SerializeField] private float secondsBeforeReturnToLobby = 5f;
    [SerializeField] private string lobbySceneName = "LobbyScene";

    public Action OnRunnersWon;
    public Action OnRunnersLost;
    public Action OnMasterWon;
    public Action OnRaceEnded;
    public Action<int, bool> OnRunnerOut;

    private bool raceEnded;

    private void Awake() => Instance = this;

    public void ReportFinish()
    {
        photonView.RPC(nameof(RPC_ReportFinish), RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void ReportEliminated(int actorNumber)
    {
        photonView.RPC(nameof(RPC_ReportEliminated), RpcTarget.All, actorNumber);
    }

    [PunRPC]
    private void RPC_ReportFinish(int actorNumber)
    {
        OnRunnerOut?.Invoke(actorNumber, true);
        EndRace(runnersWon: true);
    }

    [PunRPC]
    private void RPC_ReportEliminated(int actorNumber)
    {
        OnRunnerOut?.Invoke(actorNumber, false);
    }

    private void EndRace(bool runnersWon)
    {
        if (raceEnded) return;
        raceEnded = true;

        photonView.RPC(nameof(RPC_EndRace), RpcTarget.All, runnersWon);
    }

    [PunRPC]
    private void RPC_EndRace(bool runnersWon)
    {
        OnRaceEnded?.Invoke();

        if (runnersWon) OnRunnersWon?.Invoke();
        else
        {
            OnRunnersLost?.Invoke();
            OnMasterWon?.Invoke();
        }

        Invoke(nameof(LoadLobby), secondsBeforeReturnToLobby);
    }

    private void LoadLobby()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.LoadLevel(lobbySceneName);
    }
}