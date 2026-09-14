using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomFlowController : MonoBehaviour
{
    [SerializeField] private MainMenuPanelUI mainMenuPanel;
    [SerializeField] private CreateRoomPanelUI createRoomPanel;
    [SerializeField] private JoinRoomPanelUI joinRoomPanel;
    [SerializeField] private JoinPopupPanelUI joinPopupPanel;

    private string pendingRoomName;

    private void Awake()
    {
        mainMenuPanel.OnCreateRoomClicked += ShowCreateRoomPanel;
        mainMenuPanel.OnJoinRoomClicked += ShowJoinRoomPanel;

        createRoomPanel.OnCreateRequested += HandleCreateRequested;
        createRoomPanel.OnCancelRequested += ShowMainMenu;

        joinRoomPanel.OnBackRequested += ShowMainMenu;
        joinRoomPanel.OnRoomSelected += HandleRoomSelected;

        joinPopupPanel.OnJoinConfirmed += HandleJoinConfirmed;
        joinPopupPanel.OnCancelRequested += joinPopupPanel.Close;
    }

    private void OnEnable()
    {
        ShowMainMenu();

        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnCreateFailed += HandleFailure;
        PhotonManager.Instance.OnJoinFailed += HandleFailure;
        PhotonManager.Instance.OnRoom += HideAll;
        PhotonManager.Instance.OnDisconnectedFromServer += HandleDisconnected;
        PhotonManager.Instance.OnRoomListUpdated += joinRoomPanel.Refresh;

        joinRoomPanel.Refresh(PhotonManager.Instance.CachedRoomList);
    }

    private void OnDisable()
    {
        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnCreateFailed -= HandleFailure;
        PhotonManager.Instance.OnJoinFailed -= HandleFailure;
        PhotonManager.Instance.OnRoom -= HideAll;
        PhotonManager.Instance.OnDisconnectedFromServer -= HandleDisconnected;
        PhotonManager.Instance.OnRoomListUpdated -= joinRoomPanel.Refresh;
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.Show();
        createRoomPanel.Hide();
        joinRoomPanel.Hide();
        joinPopupPanel.Close();
    }

    private void ShowCreateRoomPanel()
    {
        mainMenuPanel.Hide();
        createRoomPanel.Show();
    }

    private void ShowJoinRoomPanel()
    {
        mainMenuPanel.Hide();
        joinRoomPanel.Show();

        if (PhotonManager.Instance != null)
        {
            joinRoomPanel.Refresh(PhotonManager.Instance.CachedRoomList);
        }
    }

    private void HideAll()
    {
        mainMenuPanel.Hide();
        createRoomPanel.Hide();
        joinRoomPanel.Hide();
        joinPopupPanel.Close();
    }

    private void HandleCreateRequested(string roomName, string password)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            createRoomPanel.SetStatus("Poné un nombre de sala");
            return;
        }
        ApplyNickname(null);

        createRoomPanel.SetStatus("Creando sala...");
        PhotonManager.Instance.CreateRoom(roomName, password);
    }

    private void HandleRoomSelected(string roomName, bool hasPassword)
    {
        pendingRoomName = roomName;
        joinPopupPanel.Open(roomName, hasPassword);
    }

    private void HandleJoinConfirmed(string nickname, string password)
    {
        if (string.IsNullOrEmpty(pendingRoomName)) return;

        ApplyNickname(nickname);
        joinPopupPanel.SetStatus("Buscando sala...");
        PhotonManager.Instance.JoinRoom(pendingRoomName, password);
    }

    private void ApplyNickname(string typed)
    {
        string nick = string.IsNullOrWhiteSpace(typed)
            ? $"Player_{Random.Range(100, 999)}"
            : typed;

        PhotonNetwork.NickName = nick;
    }

    private void HandleFailure(string message)
    {
        if (joinPopupPanel.gameObject.activeSelf) joinPopupPanel.SetStatus(message);
        else if (createRoomPanel.gameObject.activeSelf) createRoomPanel.SetStatus(message);
        else mainMenuPanel.SetStatus(message);
    }

    private void HandleDisconnected()
    {
        ShowMainMenu();
        mainMenuPanel.SetStatus("Desconectado del servidor");
    }
}