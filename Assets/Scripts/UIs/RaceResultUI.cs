using UnityEngine;
using UnityEngine.UI;

public class RaceResultUI : MonoBehaviour
{
    [SerializeField] private GameObject runnerVictoryPanel;
    [SerializeField] private GameObject runnerDefeatPanel;
    [SerializeField] private GameObject masterVictoryPanel;
    [SerializeField] private GameObject masterDefeatPanel; // "un corredor escapó"

    [Header("Salida")]
    [SerializeField] private Button returnToMenuButton; // uno solo, visible en cualquiera de los 4 paneles

    private void Awake()
    {
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(HandleReturnToMenu);
        }
    }

    private void OnEnable()
    {
        if (RaceManager.Instance == null)
        {
            Debug.LogError("RaceManager.Instance es null. Verificá que exista un GameObject con RaceManager en la escena y que su DefaultExecutionOrder corra antes que RaceResultUI.");
            return;
        }

        RaceManager.Instance.OnRunnersWon += HandleRunnersWon;
        RaceManager.Instance.OnRunnersLost += HandleRunnersLost;
    }

    private void OnDisable()
    {
        if (RaceManager.Instance == null) return;

        RaceManager.Instance.OnRunnersWon -= HandleRunnersWon;
        RaceManager.Instance.OnRunnersLost -= HandleRunnersLost;
    }

    private void HandleRunnersWon()
    {
        bool isTrapMaster = PhotonManager.Instance.IsLocalPlayerTrapMaster();
        (isTrapMaster ? masterDefeatPanel : runnerVictoryPanel).SetActive(true);
    }

    private void HandleRunnersLost()
    {
        bool isTrapMaster = PhotonManager.Instance.IsLocalPlayerTrapMaster();
        (isTrapMaster ? masterVictoryPanel : runnerDefeatPanel).SetActive(true);
    }

    private void HandleReturnToMenu()
    {
        PhotonManager.Instance.LeaveRoomIntentionally();
    }
}