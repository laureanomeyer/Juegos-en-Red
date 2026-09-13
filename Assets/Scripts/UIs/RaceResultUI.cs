using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaceResultUI : MonoBehaviour
{
    [SerializeField] private GameObject runnerVictoryPanel;
    [SerializeField] private GameObject runnerDefeatPanel;
    [SerializeField] private GameObject masterVictoryPanel;
    [SerializeField] private GameObject masterDefeatPanel;

    [Header("Contador de vivos (solo corredores)")]
    [SerializeField] private TMP_Text aliveRunnersText;

    [Header("Salida")]
    [SerializeField] private Button returnToMenuButton;

    private void Awake()
    {
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(HandleReturnToMenu);
    }

    private void OnEnable()
    {
        if (RaceManager.Instance == null)
        {
            Debug.LogError("RaceManager.Instance es null.");
            return;
        }

        RaceManager.Instance.OnRunnersWon += HandleMasterDefeat;
        RaceManager.Instance.OnRunnersLost += HandleMasterVictory;
        RaceManager.Instance.OnRunnerOut += HandlePersonalResult;
        RaceManager.Instance.OnAliveRunnersCountChanged += HandleAliveCountChanged;

        HandleAliveCountChanged(RaceManager.Instance.AliveRunnersCount); // inicializa con el valor actual, sin esperar al próximo evento
    }

    private void OnDisable()
    {
        if (RaceManager.Instance == null) return;

        RaceManager.Instance.OnRunnersWon -= HandleMasterDefeat;
        RaceManager.Instance.OnRunnersLost -= HandleMasterVictory;
        RaceManager.Instance.OnRunnerOut -= HandlePersonalResult;
        RaceManager.Instance.OnAliveRunnersCountChanged -= HandleAliveCountChanged;
    }

    // Panel del MASTER únicamente — el resultado general de la ronda.
    private void HandleMasterDefeat()
    {
        if (PhotonManager.Instance.IsLocalPlayerTrapMaster())
            masterDefeatPanel.SetActive(true);
    }

    private void HandleMasterVictory()
    {
        if (PhotonManager.Instance.IsLocalPlayerTrapMaster())
            masterVictoryPanel.SetActive(true);
    }

    // Panel PERSONAL del corredor — aparece apenas SE RESUELVE SU PROPIO destino,
    // sin esperar a que termine la ronda para el resto.
    private void HandlePersonalResult(int actorNumber, bool finished)
    {
        if (actorNumber != PhotonNetwork.LocalPlayer.ActorNumber) return;

        (finished ? runnerVictoryPanel : runnerDefeatPanel).SetActive(true);
    }

    private void HandleAliveCountChanged(int aliveCount)
    {
        if (aliveRunnersText != null)
            aliveRunnersText.text = $"Runners vivos: {aliveCount}";
    }

    private void HandleReturnToMenu()
    {
        PhotonManager.Instance.LeaveRoomIntentionally();
    }
}