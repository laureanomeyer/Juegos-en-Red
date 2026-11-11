using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PartnerStatusUI : MonoBehaviour
{
    [SerializeField] private GameObject banner;
    [SerializeField] private TMP_Text messageText;

    private void OnEnable()
    {
        var manager = PhotonManager.Instance;
        if (manager == null) return;

        manager.OnPlayerEntered += HandlePartnerBack;
        manager.OnPartnerRejoined += HandlePartnerBack;
        manager.OnPartnerDisconnected += HandlePartnerDisconnected;
        manager.OnPartnerLeft += HandlePartnerLeft;

        // Estado inicial: si todavía no hay compañero, avisamos que se espera.
        if (manager.HasPartner) Hide();
        else Show("Esperando al otro jugador...");
    }

    private void OnDisable()
    {
        var manager = PhotonManager.Instance;
        if (manager == null) return;

        manager.OnPlayerEntered -= HandlePartnerBack;
        manager.OnPartnerRejoined -= HandlePartnerBack;
        manager.OnPartnerDisconnected -= HandlePartnerDisconnected;
        manager.OnPartnerLeft -= HandlePartnerLeft;
    }

    private void HandlePartnerBack(Player partner) => Hide();

    private void HandlePartnerDisconnected(Player partner) =>
        Show("Tu compañero se desconectó. Esperando que vuelva...");

    private void HandlePartnerLeft(Player partner) =>
        Show("Tu compañero se fue. Esperando a otro jugador...");

    private void Show(string message)
    {
        if (messageText != null) messageText.text = message;
        banner.SetActive(true);
    }

    private void Hide() => banner.SetActive(false);
}