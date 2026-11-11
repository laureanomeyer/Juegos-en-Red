using UnityEngine;

/// <summary>
/// Decide qué panel del menú se muestra y le pide cosas a PhotonManager.
/// Los paneles solo avisan con eventos, la lógica vive acá.
/// </summary>
public class RoomFlowController : MonoBehaviour
{
    [SerializeField] private MainMenuPanelUI mainMenuPanel;
    [SerializeField] private CreateRoomPanelUI createRoomPanel;
    [SerializeField] private JoinRoomPanelUI joinRoomPanel;
    [SerializeField] private JoinPopupPanelUI joinPopupPanel;

    private string pendingRoomName;

    private void Awake()
    {
        mainMenuPanel.OnCreateRoomClicked += ShowCreateRoomPanel;
        mainMenuPanel.OnJoinRoomClicked += ShowJoinRoomPanel;

        createRoomPanel.OnCreateRequested += HandleCreateRequested;
        createRoomPanel.OnCancelRequested += ShowMainMenu;

        joinRoomPanel.OnBackRequested += ShowMainMenu;
        joinRoomPanel.OnRoomSelected += HandleRoomSelected;

        joinPopupPanel.OnJoinConfirmed += HandleJoinConfirmed;
        joinPopupPanel.OnCancelRequested += joinPopupPanel.Close;
    }

    private void OnEnable()
    {
        ShowMainMenu();

        if (PhotonManager.Instance == null)
        {
            Debug.LogError("[RoomFlowController] Falta el PhotonManager en la escena Menu.");
            return;
        }

        PhotonManager.Instance.OnCreateFailed += HandleFailure;
        PhotonManager.Instance.OnJoinFailed += HandleFailure;
        PhotonManager.Instance.OnRoomJoined += HideAll;
        PhotonManager.Instance.OnConnectionLost += HandleConnectionLost;
        PhotonManager.Instance.OnRoomListUpdated += joinRoomPanel.Refresh;

        joinRoomPanel.Refresh(PhotonManager.Instance.CachedRoomList);
    }

    private void OnDisable()
    {
        if (PhotonManager.Instance == null) return;

        PhotonManager.Instance.OnCreateFailed -= HandleFailure;
        PhotonManager.Instance.OnJoinFailed -= HandleFailure;
        PhotonManager.Instance.OnRoomJoined -= HideAll;
        PhotonManager.Instance.OnConnectionLost -= HandleConnectionLost;
        PhotonManager.Instance.OnRoomListUpdated -= joinRoomPanel.Refresh;
    }

    // ───────────── Navegación ─────────────
    private void ShowMainMenu()
    {
        mainMenuPanel.Show();
        createRoomPanel.Hide();
        joinRoomPanel.Hide();
        joinPopupPanel.Close();
    }

    private void ShowCreateRoomPanel()
    {
        if (!TryApplyNickname()) return;

        mainMenuPanel.Hide();
        createRoomPanel.Show();
    }

    private void ShowJoinRoomPanel()
    {
        if (!TryApplyNickname()) return;

        mainMenuPanel.Hide();
        joinRoomPanel.Show();
        joinRoomPanel.Refresh(PhotonManager.Instance.CachedRoomList);
    }

    private void HideAll()
    {
        mainMenuPanel.Hide();
        createRoomPanel.Hide();
        joinRoomPanel.Hide();
        joinPopupPanel.Close();
    }

    // ───────────── Crear / unir ─────────────
    private void HandleCreateRequested(string roomName, string password)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            createRoomPanel.SetStatus("Poné un nombre de sala");
            return;
        }

        createRoomPanel.SetStatus("Creando sala...");
        PhotonManager.Instance.CreateRoom(roomName.Trim(), password);
    }

    private void HandleRoomSelected(string roomName, bool hasPassword)
    {
        pendingRoomName = roomName;
        joinPopupPanel.Open(roomName, hasPassword);
    }

    private void HandleJoinConfirmed(string password)
    {
        if (string.IsNullOrEmpty(pendingRoomName)) return;

        joinPopupPanel.SetStatus("Buscando sala...");
        PhotonManager.Instance.JoinRoom(pendingRoomName, password);
    }

    // ───────────── Nickname ─────────────
    // Se pide una sola vez, en el menú principal, antes de crear o unirse.
    private bool TryApplyNickname()
    {
        string nick = mainMenuPanel.Nickname;

        if (string.IsNullOrWhiteSpace(nick))
        {
            mainMenuPanel.SetStatus("Poné un nickname");
            return false;
        }

        mainMenuPanel.SetStatus("");
        PhotonManager.Instance.SetNickname(nick);
        return true;
    }

    // ───────────── Errores ─────────────
    private void HandleFailure(string message)
    {
        if (joinPopupPanel.gameObject.activeSelf) joinPopupPanel.SetStatus(message);
        else if (createRoomPanel.gameObject.activeSelf) createRoomPanel.SetStatus(message);
        else mainMenuPanel.SetStatus(message);
    }

    private void HandleConnectionLost()
    {
        ShowMainMenu();
        mainMenuPanel.SetStatus("Se perdió la conexión con el servidor");
    }
}