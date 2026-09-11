using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Vive sobre el panel del menú principal. Solo conoce sus propios botones/texto
// y avisa hacia afuera con eventos - no sabe nada de Photon ni de los otros paneles.
public class MainMenuPanelUI : MonoBehaviour
{
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TMP_Text statusText;

    public event Action OnCreateRoomClicked;
    public event Action OnJoinRoomClicked;

    private void Awake()
    {
        createRoomButton.onClick.AddListener(() => OnCreateRoomClicked?.Invoke());
        joinRoomButton.onClick.AddListener(() => OnJoinRoomClicked?.Invoke());
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}