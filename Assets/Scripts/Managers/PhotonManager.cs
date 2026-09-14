using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DefaultExecutionOrder(-100)]
public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;


    public Action OnRoom;
    public Action<Dictionary<string, RoomInfo>> OnRoomListUpdated;
    public Action<string> OnJoinFailed;
    public Action<string> OnCreateFailed;
    public Action OnMasterLeftRoom;
    public Action<Player> OnRunnerLeftRoom;
    public Action OnDisconnectedFromServer;
    public Action<Player> OnPlayerLeft;
    public Action<Player> OnPlayerEntered;
    public Action OnMasterSwiched;
    public Action OnTrapMasterDisconnected;
    public Action OnTrapMasterReassigned;

    public Action OnReconnecting;
    public Action OnReconnected;


    private const string PASSWORD_KEY = "pwd";
    private const string HAS_PASSWORD_KEY = "hasPwd";
    private const string LOBBY_SCENE_NAME = "LobbyScene";
    private const string MENU_SCENE_NAME = "CreateRoomScene";

    public const string TRAP_MASTER_ACTOR_KEY = "trapMasterActor";

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private bool intentionalDisconnect;

    // Acceso de solo lectura a la lista de salas cacheada, para que la UI pueda
    // dibujar el listado apenas se habilita, sin esperar al próximo OnRoomListUpdate.
    public IReadOnlyDictionary<string, RoomInfo> CachedRoomList => cachedRoomList;

    [Header("Configuración de Zonas (Deathrun)")]
    [SerializeField] private int totalZonas = 10;
    [SerializeField] private int variantesDeZona = 4;
    [SerializeField] private bool evitarZonasConsecutivasRepetidas = true;

    public const string ZONE_SEQ_KEY = "zoneSeq";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else Instance = this;
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al Master Server de Photon");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Lobby unido, listo para crear / unirse a rooms");
    }

    public void CreateRoom(string roomName, string password, byte maxPlayers = 5)
    {
        int[] zoneSeq = GenerarSecuenciaDeZonas(totalZonas, variantesDeZona, evitarZonasConsecutivasRepetidas);

        var options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true,
            PlayerTtl = 15000,
            CustomRoomProperties = new Hashtable
        {
            { PASSWORD_KEY, password ?? "" },
            { HAS_PASSWORD_KEY, !string.IsNullOrEmpty(password) },
            { ZONE_SEQ_KEY, zoneSeq }

        },
            CustomRoomPropertiesForLobby = new[] { PASSWORD_KEY, HAS_PASSWORD_KEY }
        };

        PhotonNetwork.CreateRoom(roomName, options);
    }

    public bool IsLocalPlayerTrapMaster()
    {
        if (!PhotonNetwork.InRoom) return false;
        return GetTrapMasterActor() == PhotonNetwork.LocalPlayer.ActorNumber;
    }

    public int GetTrapMasterActor()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(TRAP_MASTER_ACTOR_KEY, out object raw))
            return (int)raw;
        return -1;
    }

    public bool IsTrapMasterInRoom()
    {
        return PhotonNetwork.CurrentRoom.Players.ContainsKey(GetTrapMasterActor());
    }

    public void JoinRoom(string roomName, string enteredPassword)
    {
        if (cachedRoomList.TryGetValue(roomName, out var info))
        {
            bool hasPassword = info.CustomProperties.TryGetValue(HAS_PASSWORD_KEY, out var hp) && (bool)hp;
            if (hasPassword)
            {
                string realPassword = info.CustomProperties.TryGetValue(PASSWORD_KEY, out var pw) ? (string)pw : "";
                if (realPassword != enteredPassword)
                {
                    OnJoinFailed?.Invoke("Contraseña incorrecta");
                    return;
                }
            }
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    // Le dice a la UI si una sala en particular tiene contraseña, sin exponer
    // las claves internas de las CustomProperties fuera de este manager.
    public bool RoomHasPassword(RoomInfo info)
    {
        return info != null && info.CustomProperties.TryGetValue(HAS_PASSWORD_KEY, out var hp) && (bool)hp;
    }

    private int[] GenerarSecuenciaDeZonas(int cantidad, int variantes, bool evitarRepetidas)
    {
        int[] seq = new int[cantidad];
        int anterior = -1;

        for (int i = 0; i < cantidad; i++)
        {
            int elegido;
            do
            {
                elegido = UnityEngine.Random.Range(0, variantes);
            }
            while (evitarRepetidas && variantes > 1 && elegido == anterior);

            seq[i] = elegido;
            anterior = elegido;
        }

        return seq;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: {message}");
        OnCreateFailed?.Invoke(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");

        string friendlyMessage = returnCode == ErrorCode.GameDoesNotExist
            ? "No existe una sala con ese nombre"
            : message;

        OnJoinFailed?.Invoke(friendlyMessage);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (var info in roomList)
        {
            if (info.RemovedFromList) cachedRoomList.Remove(info.Name);
            else cachedRoomList[info.Name] = info;
        }
        OnRoomListUpdated?.Invoke(cachedRoomList);
    }

    public override void OnJoinedRoom()
    {
        OnReconnected?.Invoke();

        Debug.Log($"Unido a sala '{PhotonNetwork.CurrentRoom.Name}' ({PhotonNetwork.CurrentRoom.PlayerCount} jugadores)");
        OnRoom?.Invoke();

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(LOBBY_SCENE_NAME);
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Salí de la sala, volviendo al menú");
        SceneManager.LoadScene(MENU_SCENE_NAME);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnPlayerEntered?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        OnPlayerLeft?.Invoke(otherPlayer);

        if (otherPlayer.ActorNumber == GetTrapMasterActor())
        {
            OnTrapMasterDisconnected?.Invoke();
            TryReassignTrapMaster();
        }
    }

    private void TryReassignTrapMaster()
    {
        if (!PhotonNetwork.InRoom) return;
        if (PhotonNetwork.CurrentRoom.Players.Count == 0) return;

        // Evitamos que todos los clientes escriban la property al mismo tiempo:
        // solo el de menor ActorNumber presente lo hace.
        int localActor = PhotonNetwork.LocalPlayer.ActorNumber;
        foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
        {
            if (kvp.Key < localActor) return;
        }

        int nuevoTrapMaster = int.MaxValue;
        foreach (var kvp in PhotonNetwork.CurrentRoom.Players)
        {
            if (kvp.Key < nuevoTrapMaster) nuevoTrapMaster = kvp.Key;
        }

        var props = new Hashtable { { TRAP_MASTER_ACTOR_KEY, nuevoTrapMaster } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        Debug.Log($"[PhotonManager] Trap Master reasignado a ActorNumber={nuevoTrapMaster}");
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Nuevo Master Client: {newMasterClient.NickName}");
        OnMasterLeftRoom?.Invoke();
    }

    public void LeaveRoomIntentionally()
    {
        intentionalDisconnect = true;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        bool esTransitorio = cause == DisconnectCause.ClientTimeout || cause == DisconnectCause.ServerTimeout;

        if (esTransitorio && !intentionalDisconnect)
        {
            OnReconnecting?.Invoke();
            PhotonNetwork.ReconnectAndRejoin();
        }
        else
        {
            OnDisconnectedFromServer?.Invoke();
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }

        intentionalDisconnect = false;
    }
    public override void OnCreatedRoom()
    {
        // Se dispara solo del lado de quien creó la sala, ya con un ActorNumber válido.
        var props = new Hashtable { { TRAP_MASTER_ACTOR_KEY, PhotonNetwork.LocalPlayer.ActorNumber } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        Debug.Log($"[PhotonManager] Sala creada. Trap master seteado como ActorNumber={PhotonNetwork.LocalPlayer.ActorNumber}");
    }
}