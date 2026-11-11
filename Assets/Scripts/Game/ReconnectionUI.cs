using UnityEngine;

public class ReconnectionUI : MonoBehaviour
{
    [SerializeField] private GameObject overlay;

    private void Awake()
    {
        overlay.SetActive(false);
    }

    private void OnEnable()
    {
        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnReconnecting += Show;
        PhotonManager.Instance.OnReconnected += Hide;
        PhotonManager.Instance.OnConnectionLost += Hide; // el manager ya te manda al menú
    }

    private void OnDisable()
    {
        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnReconnecting -= Show;
        PhotonManager.Instance.OnReconnected -= Hide;
        PhotonManager.Instance.OnConnectionLost -= Hide;
    }

    private void Show() => overlay.SetActive(true);
    private void Hide() => overlay.SetActive(false);
}