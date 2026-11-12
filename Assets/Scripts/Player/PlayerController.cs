using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
public class PlayerController : MonoBehaviourPun
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Cámara")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform cameraHolder; // pivote que inclina la cámara arriba y abajo
    [SerializeField] private float lookSensitivity = 0.1f; // grados por píxel de mouse
    [SerializeField] private float maxPitch = 85f;

    private CharacterController controller;
    private PlayerInputReader input;
    private float pitch;
    private float verticalVelocity;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputReader>();
    }

    private void Start()
    {
        if (!photonView.IsMine)
        {
            // Jugador remoto: sin input, sin cámara y sin CharacterController
            // (que si no, pelea con la posición que le manda PhotonTransformView).
            input.enabled = false;
            controller.enabled = false;
            playerCamera.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        SetupLocalPlayer();
    }

    private void SetupLocalPlayer()
    {
        // Nuestro propio cuerpo no se ve, pero sigue proyectando sombra.
        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

        // La cámara de este jugador reemplaza a la de la escena.
       /* if (Camera.main != null)
            Camera.main.gameObject.SetActive(false);

        playerCamera.gameObject.SetActive(true);
        playerCamera.tag = "MainCamera";*/
    }

    private void Update()
    {
        Look();
        Move();
    }

    private void Look()
    {
        Vector2 look = input.Look * lookSensitivity;

        transform.Rotate(0f, look.x, 0f);

        pitch = Mathf.Clamp(pitch - look.y, -maxPitch, maxPitch);
        cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        Vector2 move = input.Move;
        Vector3 direction = transform.right * move.x + transform.forward * move.y;
        if (direction.sqrMagnitude > 1f) direction.Normalize();

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}