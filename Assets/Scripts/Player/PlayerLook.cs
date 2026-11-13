using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mirar con el mouse en primera persona: gira el cuerpo en horizontal (lo ven los demás)
/// e inclina la cámara en vertical. PlayerController lo enciende solo en el jugador local.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerLook : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f; // grados por píxel de mouse
    [SerializeField] private float maxPitch = 85f;

    private Rigidbody rb;
    private Transform cameraTransform;
    private float yaw;
    private float pitch;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        var cam = GetComponentInChildren<Camera>(true);
        if (cam != null) cameraTransform = cam.transform;

        yaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        if (cameraTransform == null || Mouse.current == null) return;
        if (UIState.IsAnyOpen) return; // con una interfaz abierta no se mira

        Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity;

        yaw += delta.x;
        pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // El giro del cuerpo se aplica en el paso de física para convivir bien con el Rigidbody.
    private void FixedUpdate()
    {
        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
    }
}