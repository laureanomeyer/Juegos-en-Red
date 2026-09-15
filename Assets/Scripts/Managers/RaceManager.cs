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
        if(actorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            EndRace(runnersWon: true);
        }
    }

    [PunRPC]
    private void RPC_ReportEliminated(int actorNumber)
    {
        OnRunnerOut?.Invoke(actorNumber, false);

        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber && TodosLosCorredoresEliminados())
        {
            EndRace(runnersWon: false);
        }
    }

    private bool TodosLosCorredoresEliminados()
    {
        int trapMasterActor = PhotonManager.Instance.GetTrapMasterActor();
        Debug.Log($"[RaceManager] TodosLosCorredoresEliminados | trapMasterActor={trapMasterActor}");

        foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
        {
            if (kvp.Key == trapMasterActor) continue;
            if (kvp.Value.IsInactive) continue;

            var movement = PlayerMovement.Registry.TryGetValue(kvp.Key, out var m) ? m : null;
            if (movement == null)
            {
                Debug.Log($"[RaceManager] Actor {kvp.Key}: sin PlayerMovement en Registry, se ignora.");
                continue;
            }

            var vitals = movement.GetComponent<PlayerVitals>();
            Debug.Log($"[RaceManager] Actor {kvp.Key}: CurrentLives={vitals?.CurrentLives}");

            if (vitals != null && vitals.CurrentLives > 0)
            {
                Debug.Log($"[RaceManager] TodosLosCorredoresEliminados -> False (actor {kvp.Key} sigue vivo)");
                return false;
            }
        }

        Debug.Log("[RaceManager] TodosLosCorredoresEliminados -> True");
        return true;
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
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == lobbySceneName) return;

        if (PhotonManager.Instance.IsLocalPlayerTrapMaster())
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }

        PhotonNetwork.LoadLevel(lobbySceneName);
    }
}