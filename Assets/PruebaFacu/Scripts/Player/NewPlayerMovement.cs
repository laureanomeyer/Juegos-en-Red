using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class NewPlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference jumpAction;
    [SerializeField] InputActionReference sprintAction;

    [Header("Referencias")]
    [SerializeField] Interactor interactor;   // para saber si el input está bloqueado
    [SerializeField] HandsHolder hands;       // para ralentizar según el peso

    [Header("Velocidad")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 7f;
    [SerializeField] float acceleration = 12f;     // qué tan rápido llega a la velocidad objetivo
    [SerializeField] float airControl = 0.4f;      // 0..1, control en el aire

    [Header("Salto y gravedad")]
    [SerializeField] float jumpHeight = 1.2f;
    [SerializeField] float gravity = -20f;
    [SerializeField] float coyoteTime = 0.12f;     // margen para saltar justo después de caer de un borde

    [Header("Cargando objetos")]
    [Tooltip("Si es true, no se puede correr mientras llevás algo en las manos")]
    [SerializeField] bool noSprintWhileHolding = true;
    [Tooltip("Qué tanto reduce el salto un objeto pesado (0 = nada, 1 = muy pesado no salta)")]
    [SerializeField, Range(0f, 1f)] float heavyJumpPenalty = 0.5f;

    CharacterController _cc;
    Vector3 _horizontalVel;
    float _verticalVel;
    float _lastGroundedTime;

    void Awake() => _cc = GetComponent<CharacterController>();

    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
    }

    void Update()
    {
        bool locked = interactor != null && interactor.InputLocked;

        Vector2 input = locked ? Vector2.zero : Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);
        bool wantsSprint = !locked && sprintAction.action.IsPressed()
                           && !(noSprintWhileHolding && hands != null && hands.IsHolding);

        // --- Horizontal ---
        float speed = wantsSprint ? sprintSpeed : walkSpeed;
        if (hands != null) speed *= hands.SpeedMultiplier;

        Vector3 wishDir = (transform.right * input.x + transform.forward * input.y) * speed;

        bool grounded = _cc.isGrounded;
        if (grounded) _lastGroundedTime = Time.time;

        float accel = acceleration * (grounded ? 1f : airControl);
        _horizontalVel = Vector3.MoveTowards(_horizontalVel, wishDir, accel * Time.deltaTime * Mathf.Max(speed, 1f));

        // --- Vertical ---
        if (grounded && _verticalVel < 0f) _verticalVel = -2f;   // pegado al suelo

        bool canJump = Time.time - _lastGroundedTime <= coyoteTime;
        if (!locked && canJump && jumpAction.action.WasPressedThisFrame())
        {
            float penalty = hands != null && hands.IsHolding
                ? 1f - hands.Held.Weight * heavyJumpPenalty
                : 1f;
            _verticalVel = Mathf.Sqrt(jumpHeight * penalty * -2f * gravity);
            _lastGroundedTime = -999f;   // evita doble salto con coyote time
        }

        _verticalVel += gravity * Time.deltaTime;

        // --- Mover ---
        Vector3 motion = _horizontalVel + Vector3.up * _verticalVel;
        _cc.Move(motion * Time.deltaTime);
    }
}