using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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