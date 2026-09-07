using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class RoomUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text statusText;

    private void OnEnable()
    {
        createButton.onClick.AddListener(OnCreateClicked);
        joinButton.onClick.AddListener(OnJoinClicked);

        PhotonManager.Instance.OnCreateFailed += ShowError;
        PhotonManager.Instance.OnJoinFailed += ShowError;
        PhotonManager.Instance.OnRoom += HideUI;
        PhotonManager.Instance.OnDisconnectedFromServer += HandleDisconnected;
    }

    private void OnDisable()
    {
        createButton.onClick.RemoveListener(OnCreateClicked);
        joinButton.onClick.RemoveListener(OnJoinClicked);

        if (PhotonManager.Instance == null) return;
        PhotonManager.Instance.OnCreateFailed -= ShowError;
        PhotonManager.Instance.OnJoinFailed -= ShowError;
        PhotonManager.Instance.OnRoom -= HideUI;
        PhotonManager.Instance.OnDisconnectedFromServer -= HandleDisconnected;
    }

    private void OnCreateClicked()
    {
        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            statusText.text = "Poné un nombre de sala";
            return;
        }

        statusText.text = "Creando sala...";
        PhotonManager.Instance.CreateRoom(roomNameInput.text, passwordInput.text);
    }

    private void OnJoinClicked()
    {
        if (string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            statusText.text = "Poné un nombre de sala";
            return;
        }

        statusText.text = "Buscando sala...";
        PhotonManager.Instance.JoinRoom(roomNameInput.text, passwordInput.text);
    }

    private void ShowError(string message)
    {
        statusText.text = message;
    }

    private void HandleDisconnected()
    {
        statusText.text = "Desconectado del servidor";
        gameObject.SetActive(true); // por si estaba oculta tras entrar a una sala
    }

    private void HideUI()
    {
        gameObject.SetActive(false);
    }
}