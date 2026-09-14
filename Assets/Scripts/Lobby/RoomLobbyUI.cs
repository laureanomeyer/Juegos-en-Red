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
        if (isStarting) return;

        foreach (var entry in spawnedEntries)
        {
            Destroy(entry);
        }
        spawnedEntries.Clear();

        // Candidato a Trap Master = quien tiene el ActorNumber más bajo AHORA,
        // calculado en vivo — no depende de la Custom Property hasta que
        // efectivamente se aprieta Start.
        int candidatoMaster = PhotonManager.Instance.GetLowestActorNumberPresent();
        bool esCandidato = PhotonManager.Instance.IsLocalPlayerLowestActorPresent();

        int runnerCount = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;
            if (player.ActorNumber == candidatoMaster) continue;
            runnerCount++;
        }

        startButton.gameObject.SetActive(esCandidato);
        startButton.interactable = esCandidato && runnerCount >= 1;

        leaveButton.interactable = true;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;

            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            TMP_Text entryText = entry.GetComponentInChildren<TMP_Text>();

            string playerName = string.IsNullOrEmpty(player.NickName) ? $"Player {player.ActorNumber}" : player.NickName;
            entryText.text = player.ActorNumber == candidatoMaster ? $"Master - {playerName}" : playerName;

            spawnedEntries.Add(entry);
        }
    }

    private void OnStartButtonClicked()
    {
        if (isStarting) return;
        if (!PhotonManager.Instance.IsLocalPlayerLowestActorPresent()) return;

        int candidatoMaster = PhotonManager.Instance.GetLowestActorNumberPresent();
        int runnerCount = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsInactive) continue;
            if (player.ActorNumber == candidatoMaster) continue;
            runnerCount++;
        }
        if (runnerCount < 1) return;

        // Acá, y solo acá, se fija el Trap Master para toda la partida.
        PhotonManager.Instance.AssignTrapMasterForMatchStart();

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