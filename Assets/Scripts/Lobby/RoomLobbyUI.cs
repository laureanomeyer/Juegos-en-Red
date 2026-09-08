using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomLobbyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private string gameplaySceneName = "GameScene";

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);
    }

    private void OnEnable()
    {
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.OnPlayerEntered += HandlePlayerJoined;
            PhotonManager.Instance.OnPlayerLeft += HandlePlayerLeft;
            PhotonManager.Instance.OnMasterLeftRoom += UpdateLobbyUI;
        }
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(OnStartButtonClicked);
        leaveButton.onClick.RemoveListener(OnLeaveButtonClicked);

        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.OnPlayerEntered -= HandlePlayerJoined;
            PhotonManager.Instance.OnPlayerLeft -= HandlePlayerLeft;
            PhotonManager.Instance.OnMasterLeftRoom -= UpdateLobbyUI;
        }
    }

    private void Start()
    {
        // Ya estamos dentro de la room cuando esta escena carga,
        // así que no hace falta esperar ningún evento para dibujar la UI por primera vez.
        UpdateLobbyUI();
    }

    private void HandlePlayerJoined(Player newPlayer) => UpdateLobbyUI();
    private void HandlePlayerLeft(Player player) => UpdateLobbyUI();

    private void UpdateLobbyUI()
    {
        foreach (var entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        bool isMaster = PhotonNetwork.IsMasterClient;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        startButton.gameObject.SetActive(isMaster);
        startButton.interactable = isMaster && playerCount >= 2;

        // El botón sigue visible para el master, pero deshabilitado.
        leaveButton.interactable = !isMaster;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            TMP_Text entryText = entry.GetComponentInChildren<TMP_Text>();

            string playerName = string.IsNullOrEmpty(player.NickName) ? $"Player {player.ActorNumber}" : player.NickName;
            entryText.text = player.IsMasterClient ? $"Master - {playerName}" : playerName;

            spawnedEntries.Add(entry);
        }
    }

    private void OnStartButtonClicked()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(gameplaySceneName);
    }

    private void OnLeaveButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient) return;
        PhotonManager.Instance.LeaveRoomIntentionally();
    }
}