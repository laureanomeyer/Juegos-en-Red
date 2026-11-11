using UnityEngine;
using UnityEngine.UI;


public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button leaveButton;

    private bool isOpen;

    private void Awake()
    {
        panel.SetActive(false);
        resumeButton.onClick.AddListener(Close);
        leaveButton.onClick.AddListener(Leave);
    }

    private void Update()
    {
        if (!EscapePressed()) return;

        if (isOpen)
        {
            Close();
        }
        else if (!UIState.IsAnyOpen && UIState.LastCloseFrame != Time.frameCount)
        {
            // Si hay otra interfaz abierta (o se cerró recién con este mismo Esc), no abrimos la pausa.
            Open();
        }
    }

    private void OnDestroy()
    {
        if (isOpen) UIState.Close();
    }

    private void Open()
    {
        isOpen = true;
        panel.SetActive(true);
        UIState.Open();
    }

    private void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        panel.SetActive(false);
        UIState.Close();
    }

    private void Leave()
    {
        Close();

        if (PhotonManager.Instance != null)
            PhotonManager.Instance.LeaveRoom();
    }

    // Funciona tanto con el Input System nuevo como con el viejo.
    private static bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}