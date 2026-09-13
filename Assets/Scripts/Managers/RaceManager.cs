using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-90)]
[RequireComponent(typeof(PhotonView))]
public class RaceManager : MonoBehaviourPun
{
    public static PhotonView Instance_PhotonView; // no usado, placeholder eliminado abajo
    public static RaceManager Instance;

    [SerializeField] private float secondsBeforeReturnToLobby = 5f;
    [SerializeField] private string lobbySceneName = "LobbyScene";

    public Action<Player> OnRunnerFinished;
    public Action OnRunnersWon;   // al menos uno escapó (para la UI del master)
    public Action OnRunnersLost;  // todos murieron (para la UI del master)
    public Action OnMasterWon;

    public System.Action OnRaceEnded;

    // Se dispara para CADA corredor individualmente, apenas se resuelve su destino.
    // actorNumber = quién, finished = true si llegó a la meta, false si murió.
    public Action<int, bool> OnRunnerOut;

    // Se dispara cada vez que cambia la cantidad de corredores todavía en carrera.
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
        if (eliminatedActors.Contains(actorNumber)) return; // ya estaba muerto, no puede "llegar"
        if (!finishedActors.Add(actorNumber)) return;

        if (PhotonNetwork.CurrentRoom.Players.TryGetValue(actorNumber, out Player p))
            OnRunnerFinished?.Invoke(p);

        OnRunnerOut?.Invoke(actorNumber, true);
        NotifyAliveCount();
        CheckRaceFullyResolved();
    }

    [PunRPC]
    private void RPC_ReportEliminated(int actorNumber)
    {
        if (raceEnded) return;
        if (finishedActors.Contains(actorNumber)) return; // ya había llegado, está a salvo
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

        // Todos los corredores tienen destino resuelto: si al menos uno llegó,
        // el veredicto general es "corredores ganaron" (derrota del master).
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

        Debug.Log($"[RaceManager] RPC_EndRace recibido. ¿Este cliente es Photon MasterClient? {PhotonNetwork.IsMasterClient} | Volviendo al lobby en {secondsBeforeReturnToLobby}s si aplica.");

        if (PhotonNetwork.IsMasterClient)
        {
            Invoke(nameof(LoadLobby), secondsBeforeReturnToLobby);
        }
    }

    private void LoadLobby()
    {
        Debug.Log("[RaceManager] LoadLobby ejecutándose ahora.");
        PhotonNetwork.LoadLevel(lobbySceneName);
    }
}