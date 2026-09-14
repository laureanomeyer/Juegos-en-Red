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
        // Si ya arrancó la cuenta regresiva, no redibujamos botones
        if (isStarting) return;

        foreach (var entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        bool isTrapMaster = PhotonManager.Instance.IsLocalPlayerTrapMaster();
        bool trapMasterPresente = PhotonManager.Instance.IsTrapMasterInRoom();
        int trapMasterActor = PhotonManager.Instance.GetTrapMasterActor();

        // Contamos activos que NO sean el Trap Master, para saber si hay al menos un Runner
        int runnerCount = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;
            if (player.ActorNumber == trapMasterActor) continue;
            runnerCount++;
        }

        // Solo el Trap Master ve el botón de empezar, y solo se habilita si
        // él mismo sigue en la sala Y hay al menos un Runner esperando.
        startButton.gameObject.SetActive(isTrapMaster);
        startButton.interactable = isTrapMaster && trapMasterPresente && runnerCount >= 1;

        // Ahora cualquiera puede salir del lobby, incluido el Trap Master.
        leaveButton.interactable = true;

        // Listado de jugadores
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

        // Chequeo defensivo por si un Runner se fue justo antes del click.
        int trapMasterActor = PhotonManager.Instance.GetTrapMasterActor();
        int runnerCount = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;
            if (player.ActorNumber == trapMasterActor) continue;
            runnerCount++;
        }
        if (runnerCount < 1) return;

        // 1. Cerramos la sala inmediatamente para que nadie más se conecte
        PhotonNetwork.CurrentRoom.IsOpen = false;

        // 2. Notificamos a todos mediante RPC que arranca la cuenta regresiva
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

        // Solo el Master Client de Photon ejecuta el cambio de escena — es una
        // decisión de autoridad de RED, no de rol de gameplay, así que sigue
        // usando IsMasterClient a propósito.
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