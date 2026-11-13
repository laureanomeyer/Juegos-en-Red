using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento con Rigidbody y sincronización propia, como en el proyecto anterior.
/// Solo caminar: sin salto, empuje, agarre ni checkpoints.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float interpolationSpeed = 15f;

    private Rigidbody rb;
    private PhotonView view;
    private Vector2 moveinput;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    /// <summary>Velocidad de movimiento. Pública para poder cambiarla desde Remote Config más adelante.</summary>
    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();

        networkPosition = rb.position;
        networkRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (view.IsMine) SimulateLocalMovement();
        else InterpolateRemote();
    }

    private void SimulateLocalMovement()
    {
        // Con una interfaz abierta (pausa, paneles) el jugador no se mueve.
        Vector2 input = UIState.IsAnyOpen ? Vector2.zero : moveinput;

        Vector3 direction = transform.forward * input.y + transform.right * input.x;
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;

        Vector3 targetVel = direction * moveSpeed;
        targetVel.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVel;
    }

    private void InterpolateRemote()
    {
        rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * interpolationSpeed));
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.fixedDeltaTime * interpolationSpeed);
    }

    // Lo llama el componente PlayerInput (Behavior: Send Messages).
    public void OnMove(InputValue action)
    {
        moveinput = action.Get<Vector2>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}