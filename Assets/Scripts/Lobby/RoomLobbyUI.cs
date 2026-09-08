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
    [SerializeField] private int minPlayersToStart = 1;
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
        // Si ya arrancó la cuenta regresiva, no redibujamos botones
        if (isStarting) return;

        foreach (var entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        bool isMaster = PhotonNetwork.IsMasterClient;

        // Contamos únicamente a los jugadores activos
        int activePlayerCount = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.IsInactive) activePlayerCount++;
        }

        // El Master ve el botón, y se habilita solo si hay 2 o más jugadores activos
        startButton.gameObject.SetActive(isMaster);
        startButton.interactable = isMaster && activePlayerCount >= minPlayersToStart;

        // Solo los Runners pueden salirse de forma normal
        leaveButton.interactable = !isMaster;

        // Listado de jugadores
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;

            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            TMP_Text entryText = entry.GetComponentInChildren<TMP_Text>();

            string playerName = string.IsNullOrEmpty(player.NickName) ? $"Player {player.ActorNumber}" : player.NickName;
            entryText.text = player.IsMasterClient ? $"Master - {playerName}" : playerName;

            spawnedEntries.Add(entry);
        }
    }

    private void OnStartButtonClicked()
    {
        if (!PhotonNetwork.IsMasterClient || isStarting) return;

        // 1. Cerramos la sala inmediatamente para que nadie más se conecte
        PhotonNetwork.CurrentRoom.IsOpen = false;

        // 2. Notificamos a todos mediante RPC que arranca la cuenta regresiva de 5 segundos
        photonView.RPC(nameof(RPC_StartCountdown), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_StartCountdown()
    {
        isStarting = true;

        // Bloqueamos los botones para todos
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

        // 3. Solo el Master Client ejecuta el cambio de escena
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameplaySceneName);
        }
    }

    private void OnLeaveButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient || isStarting) return;
        PhotonManager.Instance.LeaveRoomIntentionally();
    }
}