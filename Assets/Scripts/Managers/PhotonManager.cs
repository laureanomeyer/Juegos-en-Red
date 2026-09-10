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

    private const string PASSWORD_KEY = "pwd";
    private const string HAS_PASSWORD_KEY = "hasPwd";
    private const string LOBBY_SCENE_NAME = "LobbyScene";
    private const string MENU_SCENE_NAME = "CreateRoomScene";

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
            { ZONE_SEQ_KEY, zoneSeq } // <-- se genera UNA vez, acá, y viaja con la sala
        },
            CustomRoomPropertiesForLobby = new[] { PASSWORD_KEY, HAS_PASSWORD_KEY }
            // zoneSeq NO va en CustomRoomPropertiesForLobby a propósito:
            // no hace falta mandarla a todo el lobby, solo a quien entra a la sala.
        };

        PhotonNetwork.CreateRoom(roomName, options);
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
        Debug.Log($"Unido a sala '{PhotonNetwork.CurrentRoom.Name}' ({PhotonNetwork.CurrentRoom.PlayerCount} jugadores)");
        OnRoom?.Invoke();

        // Solo el Master Client dispara el cambio de escena.
        // Gracias a AutomaticallySyncScene, el resto de los jugadores
        // (los que ya están o los que se unan después) cargan la misma escena solos.
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
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Nuevo Master Client: {newMasterClient.NickName}");
        OnMasterLeftRoom?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Desconectado de Photon: {cause}");

        bool esTransitorio = cause == DisconnectCause.ClientTimeout
                           || cause == DisconnectCause.ServerTimeout;

        if (esTransitorio && !intentionalDisconnect)
        {
            PhotonNetwork.ReconnectAndRejoin();
        }
        else
        {
            OnDisconnectedFromServer?.Invoke();
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }

        intentionalDisconnect = false;
    }

    public void LeaveRoomIntentionally()
    {
        intentionalDisconnect = true;
        PhotonNetwork.LeaveRoom();
    }
}