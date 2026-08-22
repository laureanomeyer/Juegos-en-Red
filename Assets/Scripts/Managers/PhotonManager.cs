using Photon.Pun;
using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance;
    public Action OnRoom;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        } 
        else Instance = this;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Taichu joined the scene");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        PhotonNetwork.JoinRandomOrCreateRoom(roomName: "The Playlist");
    }

    public override void OnJoinedRoom()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        Debug.Log("Taichu joined the lobby");

        OnRoom?.Invoke();
    }
}
