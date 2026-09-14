using UnityEngine;

public class ReconnectionUI : MonoBehaviour
{
    [SerializeField] private GameObject reconnectingOverlay;

    private void Awake()
    {
        if (reconnectingOverlay != null)
        {
            reconnectingOverlay.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnReconnecting += HandleReconnecting;
        PhotonManager.Instance.OnReconnected += HandleReconnected;
        PhotonManager.Instance.OnDisconnectedFromServer += HandleGaveUp;
    }

    private void OnDisable()
    {
        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnReconnecting -= HandleReconnecting;
        PhotonManager.Instance.OnReconnected -= HandleReconnected;
        PhotonManager.Instance.OnDisconnectedFromServer -= HandleGaveUp;
    }

    private void HandleReconnecting()
    {
        if (reconnectingOverlay != null)
        {
            reconnectingOverlay.SetActive(true);
        }
    }

    private void HandleReconnected()
    {
        if (reconnectingOverlay != null)
        {
            reconnectingOverlay.SetActive(false);
        }
    }

    // Se dispara si el intento de reconexión falla del todo (ClientTimeout/ServerTimeout agotados, o cualquier otra causa no transitoria) y PhotonManager ya te mandó al menú.
    private void HandleGaveUp()
    {
        if (reconnectingOverlay != null)
        {
            reconnectingOverlay.SetActive(false);
        }
    }
}