using UnityEngine;
using UnityEngine.InputSystem;

public class TankMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 100f;

    [SerializeField] private InputActionReference moveAction; // Vector2 (WASD/Flechas/Stick)

    private Rigidbody rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }

    private void Update()
    {
        input = moveAction.action.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        // input.y = adelante/atrás, input.x = rotación
        Vector3 move = transform.forward * input.y * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        float rotation = input.x * rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotation, 0f));
    }
}