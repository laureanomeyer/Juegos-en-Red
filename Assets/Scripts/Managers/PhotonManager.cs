using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
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

    private const string PASSWORD_KEY = "pwd";
    private const string HAS_PASSWORD_KEY = "hasPwd";

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private bool intentionalDisconnect;


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

    public void CreateRoom(string roomName, string password, byte maxPlayers = 4)
    {
        var options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true,
            PlayerTtl = 15000,
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

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: {message}");
        OnCreateFailed?.Invoke(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
        OnJoinFailed?.Invoke(message);
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
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        OnRunnerLeftRoom?.Invoke(otherPlayer);
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
        }

        intentionalDisconnect = false;
    }

    public void LeaveRoomIntentionally()
    {
        intentionalDisconnect = true;
        PhotonNetwork.LeaveRoom();
    }
}
