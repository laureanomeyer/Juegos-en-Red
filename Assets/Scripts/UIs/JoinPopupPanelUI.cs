using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Vive sobre el popup de "nombre + contraseña" que aparece al elegir una sala
// de la lista. El campo de contraseña se prende o apaga solo según la sala.
public class JoinPopupPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private GameObject passwordContainer; // agrupa label + input de contraseña
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text statusText;

    public event Action<string, string> OnJoinConfirmed; // nickname, password
    public event Action OnCancelRequested;

    private bool currentHasPassword;

    private void Awake()
    {
        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(() => OnCancelRequested?.Invoke());
    }

    private void HandleConfirm()
    {
        string password = currentHasPassword && passwordInput != null ? passwordInput.text : "";
        string nickname = nicknameInput != null ? nicknameInput.text : "";
        OnJoinConfirmed?.Invoke(nickname, password);
    }

    public void Open(string roomName, bool hasPassword)
    {
        currentHasPassword = hasPassword;

        if (roomNameText != null) roomNameText.text = roomName;
        if (nicknameInput != null) nicknameInput.text = "";
        if (passwordInput != null) passwordInput.text = "";
        if (passwordContainer != null) passwordContainer.SetActive(hasPassword);
        SetStatus("");

        gameObject.SetActive(true);
    }

    public void Close() => gameObject.SetActive(false);

    public void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}