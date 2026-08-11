using UnityEngine;
using UnityEngine.InputSystem;

public class TankShooter : MonoBehaviour
{
    [SerializeField] private InputActionReference fireAction; // Button

    private void OnEnable()
    {
        fireAction.action.Enable();
    }

    private void OnDisable()
    {
        fireAction.action.Disable();
    }

    private void Update()
    {
        if (fireAction.action.WasPressedThisFrame())
        {
            Debug.Log("Disparo!");
        }
    }
}
