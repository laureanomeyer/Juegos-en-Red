using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject loginPanel;

    [Header("Inputs & Botones (Crear sala)")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button createButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Lista de salas")]
    [SerializeField] private Transform roomListContent;
    [SerializeField] private RoomListEntryUI roomListEntryPrefab;

    [Header("Prompt de contraseña (Unirse)")]
    [SerializeField] private GameObject joinPasswordPanel;
    [SerializeField] private TMP_Text joinPasswordRoomNameText;
    [SerializeField] private TMP_InputField joinPasswordInput;
    [SerializeField] private Button joinConfirmButton;
    [SerializeField] private Button joinCancelButton;

    private readonly List<GameObject> spawnedRoomEntries = new List<GameObject>();
    private string pendingRoomName;

    private void OnEnable()
    {
        createButton.onClick.RemoveListener(OnCreateClicked);
        createButton.onClick.AddListener(OnCreateClicked);

        if (joinConfirmButton != null)
        {
            joinConfirmButton.onClick.RemoveListener(OnJoinConfirmClicked);
            joinConfirmButton.onClick.AddListener(OnJoinConfirmClicked);
        }

        if (joinCancelButton != null)
        {
            joinCancelButton.onClick.RemoveListener(OnJoinCancelClicked);
            joinCancelButton.onClick.AddListener(OnJoinCancelClicked);
        }

        if (joinPasswordPanel != null) joinPasswordPanel.SetActive(false);

        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnCreateFailed += ShowError;
        PhotonManager.Instance.OnJoinFailed += ShowError;
        PhotonManager.Instance.OnRoom += HideUI;
        PhotonManager.Instance.OnDisconnectedFromServer += HandleDisconnected;
        PhotonManager.Instance.OnRoomListUpdated += HandleRoomListUpdated;

        // Por si ya había salas cacheadas antes de que esta UI se habilitara
        // (ej: se vuelve al menú y el lobby ya tenía datos).
        HandleRoomListUpdated(PhotonManager.Instance.CachedRoomList);
    }

    private void OnDisable()
    {
        createButton.onClick.RemoveListener(OnCreateClicked);

        if (joinConfirmButton != null) joinConfirmButton.onClick.RemoveListener(OnJoinConfirmClicked);
        if (joinCancelButton != null) joinCancelButton.onClick.RemoveListener(OnJoinCancelClicked);

        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnCreateFailed -= ShowError;
        PhotonManager.Instance.OnJoinFailed -= ShowError;
        PhotonManager.Instance.OnRoom -= HideUI;
        PhotonManager.Instance.OnDisconnectedFromServer -= HandleDisconnected;
        PhotonManager.Instance.OnRoomListUpdated -= HandleRoomListUpdated;
    }

    private void ApplyNickname()
    {
        string nick = string.IsNullOrWhiteSpace(nicknameInput.text)
            ? $"Player_{Random.Range(100, 999)}"
            : nicknameInput.text;

        PhotonNetwork.NickName = nick;
    }

    private void OnCreateClicked()
    {
        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            statusText.text = "Poné un nombre de sala";
            return;
        }

        ApplyNickname();
        statusText.text = "Creando sala...";
        PhotonManager.Instance.CreateRoom(roomNameInput.text, passwordInput.text);
    }

    // Se llama cada vez que Photon actualiza la lista de salas del lobby.
    // Redibuja todas las entradas (mismo criterio simple que usa RoomLobbyUI con la lista de jugadores).
    private void HandleRoomListUpdated(IReadOnlyDictionary<string, RoomInfo> rooms)
    {
        foreach (var entry in spawnedRoomEntries)
        {
            Destroy(entry);
        }
        spawnedRoomEntries.Clear();

        if (rooms == null || roomListEntryPrefab == null || roomListContent == null) return;

        foreach (var kvp in rooms)
        {
            RoomInfo info = kvp.Value;
            if (info.RemovedFromList) continue;

            RoomListEntryUI entryUI = Instantiate(roomListEntryPrefab, roomListContent);
            bool hasPassword = PhotonManager.Instance.RoomHasPassword(info);

            entryUI.Setup(info.Name, info.PlayerCount, info.MaxPlayers, hasPassword, info.IsOpen, OnRoomEntrySelected);

            spawnedRoomEntries.Add(entryUI.gameObject);
        }
    }

    // El jugador clickeó una sala de la lista (no llena / no cerrada): pedimos la contraseña.
    private void OnRoomEntrySelected(string roomName)
    {
        pendingRoomName = roomName;

        if (joinPasswordRoomNameText != null) joinPasswordRoomNameText.text = roomName;
        if (joinPasswordInput != null) joinPasswordInput.text = "";
        if (statusText != null) statusText.text = "";

        if (joinPasswordPanel != null) joinPasswordPanel.SetActive(true);
    }

    private void OnJoinConfirmClicked()
    {
        if (string.IsNullOrEmpty(pendingRoomName)) return;

        ApplyNickname();
        statusText.text = "Buscando sala...";

        string enteredPassword = joinPasswordInput != null ? joinPasswordInput.text : "";
        PhotonManager.Instance.JoinRoom(pendingRoomName, enteredPassword);

        if (joinPasswordPanel != null) joinPasswordPanel.SetActive(false);
    }

    private void OnJoinCancelClicked()
    {
        pendingRoomName = null;
        if (joinPasswordPanel != null) joinPasswordPanel.SetActive(false);
    }

    private void ShowError(string message)
    {
        statusText.text = message;
    }

    private void HandleDisconnected()
    {
        statusText.text = "Desconectado del servidor";
        loginPanel.SetActive(true);
    }

    private void HideUI()
    {
        loginPanel.SetActive(false);
        if (joinPasswordPanel != null) joinPasswordPanel.SetActive(false);
    }
}