using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class RoomLobbyUI : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private string gameplaySceneName = "GameScene";

    [Header("Configuración de Arranque")]
    [SerializeField] private float countdownDuration = 5f;

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();
    private bool isStarting = false;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(OnStartButtonClicked);
        leaveButton.onClick.RemoveListener(OnLeaveButtonClicked);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.OnPlayerEntered += HandlePlayerJoined;
            PhotonManager.Instance.OnPlayerLeft += HandlePlayerLeft;
            PhotonManager.Instance.OnMasterLeftRoom += UpdateLobbyUI;
            PhotonManager.Instance.OnTrapMasterReassigned += UpdateLobbyUI;
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.OnPlayerEntered -= HandlePlayerJoined;
            PhotonManager.Instance.OnPlayerLeft -= HandlePlayerLeft;
            PhotonManager.Instance.OnMasterLeftRoom -= UpdateLobbyUI;
            PhotonManager.Instance.OnTrapMasterReassigned -= UpdateLobbyUI;
        }
    }

    private void Start()
    {
        UpdateLobbyUI();
    }

    private void HandlePlayerJoined(Player newPlayer) => UpdateLobbyUI();
    private void HandlePlayerLeft(Player player) => UpdateLobbyUI();

    private void UpdateLobbyUI()
    {
        if (isStarting) return;

        foreach (var entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        bool isTrapMaster = PhotonManager.Instance.IsLocalPlayerTrapMaster();
        bool trapMasterPresente = PhotonManager.Instance.IsTrapMasterInRoom();
        int trapMasterActor = PhotonManager.Instance.GetTrapMasterActor();
        int runnerCount = 0;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;
            if (player.ActorNumber == trapMasterActor) continue;
            runnerCount++;
        }
        startButton.gameObject.SetActive(isTrapMaster);
        startButton.interactable = isTrapMaster && trapMasterPresente && runnerCount >= 1;
        leaveButton.interactable = true;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;

            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            TMP_Text entryText = entry.GetComponentInChildren<TMP_Text>();

            string playerName = string.IsNullOrEmpty(player.NickName) ? $"Player {player.ActorNumber}" : player.NickName;
            entryText.text = player.ActorNumber == trapMasterActor ? $"Master - {playerName}" : playerName;

            spawnedEntries.Add(entry);
        }
    }

    private void OnStartButtonClicked()
    {
        if (isStarting) return;
        if (!PhotonManager.Instance.IsLocalPlayerTrapMaster()) return;

        int trapMasterActor = PhotonManager.Instance.GetTrapMasterActor();
        int runnerCount = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;
            if (player.ActorNumber == trapMasterActor) continue;
            runnerCount++;
        }
        if (runnerCount < 1) return;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartCountdown()
    {
        isStarting = true;
        startButton.interactable = false;
        leaveButton.interactable = false;

        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        float remaining = countdownDuration;

        while (remaining > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = $"Iniciando partida en: {Mathf.CeilToInt(remaining)}s";
            }
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        if (countdownText != null)
        {
            countdownText.text = "¡Cargando!";
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameplaySceneName);
        }
    }

    private void OnLeaveButtonClicked()
    {
        if (isStarting) return;
        PhotonManager.Instance.LeaveRoomIntentionally();
    }
}