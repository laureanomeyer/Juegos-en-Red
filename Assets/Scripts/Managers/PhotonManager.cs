using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DefaultExecutionOrder(-100)]
public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance { get; private set; }

    // ───────────── Eventos ─────────────
    public event Action OnRoomJoined;
    public event Action<IReadOnlyDictionary<string, RoomInfo>> OnRoomListUpdated;
    public event Action<string> OnCreateFailed;
    public event Action<string> OnJoinFailed;
    public event Action<Player> OnPlayerEntered;      // entra alguien nuevo
    public event Action<Player> OnPartnerDisconnected; // se cayó (lugar reservado por el TTL)
    public event Action<Player> OnPartnerRejoined;     // volvió dentro del TTL
    public event Action<Player> OnPartnerLeft;         // se fue de verdad (salió o venció el TTL)
    public event Action OnReconnecting;    // yo me caí, intentando volver
    public event Action OnReconnected;     // volví a la sala
    public event Action OnConnectionLost;  // no se pudo volver, te mandó al menú

    // ───────────── Config ─────────────
    [Header("Escenas")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string gameSceneName = "Game";

    [Header("Sala")]
    [SerializeField] private int playerTtlMs = 15000; // tiempo que se reserva el lugar de un jugador caído
    [SerializeField] private float recoverDelay = 2f;

    private const byte MAX_PLAYERS = 2;
    private const string PASSWORD_KEY = "pwd";
    private const string HAS_PASSWORD_KEY = "hasPwd";
    private const string NOT_READY_MSG = "Todavía conectando al servidor, esperá un segundo e intentá de nuevo";

    // ───────────── Estado interno ─────────────
    private readonly Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private readonly HashSet<int> inactiveActors = new HashSet<int>();
    private bool wasInRoom;
    private bool isRejoining;

    // ───────────── Consultas públicas ─────────────
    public IReadOnlyDictionary<string, RoomInfo> CachedRoomList => cachedRoomList;
    public bool IsReady => PhotonNetwork.IsConnectedAndReady;
    public bool IsInRoom => PhotonNetwork.InRoom;
    public bool IsMaster => PhotonNetwork.IsMasterClient;

    public int ActivePlayerCount
    {
        get
        {
            if (!PhotonNetwork.InRoom) return 0;
            int count = 0;
            foreach (var p in PhotonNetwork.PlayerList)
                if (!p.IsInactive) count++;
            return count;
        }
    }

    public bool HasPartner => ActivePlayerCount >= MAX_PLAYERS;

    // ───────────── Ciclo de vida ─────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    // ───────────── Acciones públicas ─────────────
    public void SetNickname(string typed)
    {
        PhotonNetwork.NickName = string.IsNullOrWhiteSpace(typed)
            ? $"Player_{UnityEngine.Random.Range(100, 999)}"
            : typed.Trim();
    }

    public void CreateRoom(string roomName, string password)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            OnCreateFailed?.Invoke(NOT_READY_MSG);
            return;
        }

        var options = new RoomOptions
        {
            MaxPlayers = MAX_PLAYERS,
            IsVisible = true,
            IsOpen = true,
            PlayerTtl = playerTtlMs,
            EmptyRoomTtl = playerTtlMs, // si se caen los dos, la sala sobrevive el mismo tiempo
            CustomRoomProperties = new Hashtable
            {
                { PASSWORD_KEY, password ?? "" },
                { HAS_PASSWORD_KEY, !string.IsNullOrEmpty(password) }
            },
            CustomRoomPropertiesForLobby = new[] { PASSWORD_KEY, HAS_PASSWORD_KEY }
        };

        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoom(string roomName, string enteredPassword)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            OnJoinFailed?.Invoke(NOT_READY_MSG);
            return;
        }

        if (cachedRoomList.TryGetValue(roomName, out var info) && RoomHasPassword(info))
        {
            string realPassword = info.CustomProperties.TryGetValue(PASSWORD_KEY, out var pw) ? (string)pw : "";
            if (realPassword != enteredPassword)
            {
                OnJoinFailed?.Invoke("Contraseña incorrecta");
                return;
            }
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    public bool RoomHasPassword(RoomInfo info)
    {
        return info != null
            && info.CustomProperties.TryGetValue(HAS_PASSWORD_KEY, out var hp)
            && (bool)hp;
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            GoToMenu();
            return;
        }

        PhotonNetwork.LeaveRoom(becomeInactive: false);
    }

    // ───────────── Conexión y lobby de Photon ─────────────
    public override void OnConnectedToMaster()
    {
        // Si estamos reconectando a una sala, Photon sigue solo con el rejoin.
        if (isRejoining) return;

        cachedRoomList.Clear();
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[PhotonManager] En el lobby de Photon, listo para crear / unirse a salas");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // Lo cortamos nosotros a propósito, no hay nada que recuperar.
        if (cause == DisconnectCause.DisconnectByClientLogic) return;

        bool transient = cause == DisconnectCause.ClientTimeout || cause == DisconnectCause.ServerTimeout;

        if (transient && wasInRoom && !isRejoining)
        {
            isRejoining = true;
            OnReconnecting?.Invoke();

            if (!PhotonNetwork.ReconnectAndRejoin())
                GiveUpAndReturnToMenu();
            return;
        }

        GiveUpAndReturnToMenu();
    }

    private void GiveUpAndReturnToMenu()
    {
        isRejoining = false;
        wasInRoom = false;
        inactiveActors.Clear();

        OnConnectionLost?.Invoke();
        GoToMenu();

        CancelInvoke(nameof(RecoverConnection));
        Invoke(nameof(RecoverConnection), recoverDelay);
    }

    // Deja el cliente de nuevo en el lobby de Photon, con un delay para no entrar en loop si no hay internet.
    private void RecoverConnection()
    {
        var state = PhotonNetwork.NetworkClientState;

        if (state == ClientState.ConnectedToMasterServer)
            PhotonNetwork.JoinLobby();
        else if (state == ClientState.Disconnected || state == ClientState.PeerCreated)
            PhotonNetwork.ConnectUsingSettings();
    }

    // ───────────── Salas ─────────────
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var info in roomList)
        {
            if (info.RemovedFromList) cachedRoomList.Remove(info.Name);
            else cachedRoomList[info.Name] = info;
        }

        OnRoomListUpdated?.Invoke(cachedRoomList);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[PhotonManager] No se pudo crear la sala: {message}");
        OnCreateFailed?.Invoke(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[PhotonManager] No se pudo unir a la sala: {message}");

        // El rejoin falló (por ejemplo, la sala ya venció): no hay vuelta, al menú.
        if (isRejoining)
        {
            GiveUpAndReturnToMenu();
            return;
        }

        string friendly = returnCode == ErrorCode.GameDoesNotExist
            ? "No existe una sala con ese nombre"
            : message;

        OnJoinFailed?.Invoke(friendly);
    }

    public override void OnJoinedRoom()
    {
        wasInRoom = true;

        // Volví después de una caída: no recargo escena ni reinicio nada.
        if (isRejoining)
        {
            isRejoining = false;
            OnReconnected?.Invoke();
            return;
        }

        inactiveActors.Clear();
        Debug.Log($"[PhotonManager] Unido a '{PhotonNetwork.CurrentRoom.Name}' ({PhotonNetwork.CurrentRoom.PlayerCount}/{MAX_PLAYERS})");
        OnRoomJoined?.Invoke();

        // El Master lleva a todos a la escena de juego (el resto se sincroniza solo).
        if (PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name != gameSceneName)
            PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnLeftRoom()
    {
        wasInRoom = false;
        isRejoining = false;
        inactiveActors.Clear();

        GoToMenu();
    }

    // ───────────── Compañero ─────────────
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (inactiveActors.Remove(newPlayer.ActorNumber))
            OnPartnerRejoined?.Invoke(newPlayer);
        else
            OnPlayerEntered?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Con PlayerTtl Photon avisa dos veces: al caerse (IsInactive) y cuando vence el tiempo.
        if (otherPlayer.IsInactive)
        {
            inactiveActors.Add(otherPlayer.ActorNumber);
            OnPartnerDisconnected?.Invoke(otherPlayer);
        }
        else
        {
            inactiveActors.Remove(otherPlayer.ActorNumber);
            OnPartnerLeft?.Invoke(otherPlayer);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"[PhotonManager] Nuevo Master: {newMasterClient.NickName}");
    }

    // ───────────── Util ─────────────
    private void GoToMenu()
    {
        if (SceneManager.GetActiveScene().name != menuSceneName)
            SceneManager.LoadScene(menuSceneName);
    }
}