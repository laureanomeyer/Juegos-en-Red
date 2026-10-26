using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionReference lookAction;

    [Header("Referencias")]
    [SerializeField] Transform playerBody;     // el objeto raíz del jugador (rota en Y)
    [SerializeField] Transform cameraPivot;    // la cámara o un padre de la cámara (rota en X)
    [SerializeField] Interactor interactor;

    [Header("Ajustes")]
    [SerializeField] float sensitivity = 0.12f;   // grados por unidad de delta del mouse
    [SerializeField] float minPitch = -85f;
    [SerializeField] float maxPitch = 85f;

    float _pitch;

    void OnEnable() => lookAction.action.Enable();
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            var a = lookAction.action;
            Debug.Log($"[Look] asset: {a.actionMap.asset.name} | habilitada: {a.enabled} | " +
                      $"bindings: {a.bindings.Count} | controles activos: {a.controls.Count}");
            Debug.Log($"[Look] Mouse.current: {(Mouse.current != null ? "OK" : "NULL")}");
        }

    void LateUpdate()
    {
        if (interactor != null && interactor.InputLocked) return;

        // El delta del mouse ya es por frame: NO se multiplica por Time.deltaTime
        Vector2 delta = lookAction.action.ReadValue<Vector2>() * sensitivity;

        if (delta != Vector2.zero) Debug.Log($"Look: {delta}");

        _pitch = Mathf.Clamp(_pitch - delta.y, minPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        playerBody.Rotate(Vector3.up * delta.x, Space.Self);
    }
}