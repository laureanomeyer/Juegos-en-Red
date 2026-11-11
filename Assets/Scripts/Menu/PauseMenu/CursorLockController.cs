using UnityEngine;

public class CursorLockController : MonoBehaviour
{
    private void OnEnable()
    {
        UIState.OnChanged += Apply;
        Apply(UIState.IsAnyOpen);
    }

    private void OnDisable()
    {
        UIState.OnChanged -= Apply;
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Apply(bool uiOpen)
    {
        Cursor.lockState = uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = uiOpen;
    }
}