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

    [Header("Contador de vivos")]
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

        HandleAliveCountChanged(RaceManager.Instance.AliveRunnersCount);
    }

    private void OnDisable()
    {
        if (RaceManager.Instance == null) return;

        RaceManager.Instance.OnRunnersWon -= HandleMasterDefeat;
        RaceManager.Instance.OnRunnersLost -= HandleMasterVictory;
        RaceManager.Instance.OnRunnerOut -= HandlePersonalResult;
        RaceManager.Instance.OnAliveRunnersCountChanged -= HandleAliveCountChanged;
    }

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