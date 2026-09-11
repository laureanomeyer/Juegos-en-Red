using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Vive sobre el panel de "crear sala". Solo conoce sus propios inputs/botones.
// Contraseña vacía = sala pública (eso lo decide PhotonManager, no este script).
public class CreateRoomPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TMP_Text statusText;

    public event Action<string, string> OnCreateRequested; // roomName, password
    public event Action OnCancelRequested;

    private void Awake()
    {
        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(() => OnCancelRequested?.Invoke());
    }

    private void HandleConfirm()
    {
        OnCreateRequested?.Invoke(roomNameInput.text, passwordInput.text);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        roomNameInput.text = "";
        passwordInput.text = "";
        SetStatus("");
    }

    public void Hide() => gameObject.SetActive(false);

    public void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}