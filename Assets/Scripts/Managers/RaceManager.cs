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

    [SerializeField] private float secondsBeforeReturnToLobby = 5f;
    [SerializeField] private string lobbySceneName = "LobbyScene";

    public Action OnRunnersWon;
    public Action OnRunnersLost;
    public Action OnMasterWon;
    public System.Action OnRaceEnded;
    public Action<int, bool> OnRunnerOut;
    public Action<int> OnAliveRunnersCountChanged;
    private readonly HashSet<int> finishedActors = new HashSet<int>();
    private readonly HashSet<int> eliminatedActors = new HashSet<int>();
    private int totalRunners;
    private bool raceEnded;

    public int AliveRunnersCount { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        int trapMasterActor = PhotonManager.Instance.GetTrapMasterActor();
        totalRunners = 0;
        foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
        {
            if (kvp.Key != trapMasterActor) totalRunners++;
        }

        AliveRunnersCount = totalRunners;
    }

    private void OnEnable()
    {
        if (PhotonManager.Instance != null)
            PhotonManager.Instance.OnPlayerLeft += HandlePlayerLeftRoom;
    }

    private void OnDisable()
    {
        if (PhotonManager.Instance != null)
            PhotonManager.Instance.OnPlayerLeft -= HandlePlayerLeftRoom;
    }

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
        if (raceEnded) return;
        if (actorNumber == PhotonManager.Instance.GetTrapMasterActor()) return;
        if (eliminatedActors.Contains(actorNumber)) return;
        if (!finishedActors.Add(actorNumber)) return;

        OnRunnerOut?.Invoke(actorNumber, true);
        NotifyAliveCount();
        CheckRaceFullyResolved();
    }

    [PunRPC]
    private void RPC_ReportEliminated(int actorNumber)
    {
        if (raceEnded) return;
        if (finishedActors.Contains(actorNumber)) return;
        if (!eliminatedActors.Add(actorNumber)) return;

        OnRunnerOut?.Invoke(actorNumber, false);
        NotifyAliveCount();
        CheckRaceFullyResolved();
    }

    private void NotifyAliveCount()
    {
        int alive = totalRunners - finishedActors.Count - eliminatedActors.Count;
        AliveRunnersCount = Mathf.Max(0, alive);
        OnAliveRunnersCountChanged?.Invoke(AliveRunnersCount);
    }

    private void CheckRaceFullyResolved()
    {
        if (totalRunners <= 0) return;
        if (finishedActors.Count + eliminatedActors.Count < totalRunners) return;
        if (!EsElActorMasBajoPresente()) return;

        bool anyoneEscaped = finishedActors.Count > 0;
        EndRace(anyoneEscaped);
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

        bool esTrapMaster = PhotonManager.Instance.IsLocalPlayerTrapMaster();
        bool trapMasterPresente = PhotonManager.Instance.IsTrapMasterInRoom();

        // Si el Trap Master ya no está en la sala, la Custom Property nunca se
        // actualiza sola durante la carrera (a propósito, para no romper el
        // conteo de corredores) — así que el actor de menor ActorNumber
        // presente toma la responsabilidad de volver al lobby. La reasignación
        // real del rol la maneja PhotonManager al cargar LobbyScene, no acá.
        bool debeCargarLobby = esTrapMaster || (!trapMasterPresente && EsElActorMasBajoPresente());

        if (debeCargarLobby)
        {
            Invoke(nameof(LoadLobby), secondsBeforeReturnToLobby);
        }
    }

    private bool EsElActorMasBajoPresente()
    {
        int localActor = PhotonNetwork.LocalPlayer.ActorNumber;

        foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
        {
            if (kvp.Key < localActor) return false;
        }

        return true;
    }

    private void HandlePlayerLeftRoom(Player p)
    {
        if (raceEnded) return;

        int trapMasterActor = PhotonManager.Instance.GetTrapMasterActor();
        int corredoresEnSala = 0;

        foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
        {
            if (kvp.Key != trapMasterActor) corredoresEnSala++;
        }

        if (corredoresEnSala > 0) return;
        if (!EsElActorMasBajoPresente()) return;

        AliveRunnersCount = 0;
        OnAliveRunnersCountChanged?.Invoke(0);

        EndRace(runnersWon: false);
    }

    private void LoadLobby()
    {
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.LoadLevel(lobbySceneName);
    }
}