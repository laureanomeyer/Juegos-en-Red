using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float knockdownDuration = 1f;
    [SerializeField] private float grabSlowMultiplier = 0.4f;
    [SerializeField] private float interpolationSpeed = 15f;
    [SerializeField] private Transform mainPlayerCamerTransform;
    [SerializeField] private Transform runnerCameraTransform;

    [Header("Salto (solo Runners)")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    private Transform cameraTransform;

    private Rigidbody rb;
    private Vector2 moveinput;
    private PhotonView view;

    private bool isKnockedDown;
    private float knockdownTimer;

    private bool isBeingGrabbed;
    private bool isGrabbingSomeone;
    private Vector3 windVelocity = Vector3.zero;
    private float windDecay = 20f;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    private bool isOut;
    private bool raceEnded;
    private bool raceFullyEnded;
    private bool isAttemptingGrab;

    private bool isRunner;
    private bool jumpRequested;
    private bool isGrounded;

    private Vector3 checkpointPosition;

    public static readonly Dictionary<int, PlayerMovement> Registry = new Dictionary<int, PlayerMovement>();

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();

        networkPosition = rb.position;
        networkRotation = transform.rotation;
        checkpointPosition = rb.position;

        isRunner = !PhotonManager.Instance.IsLocalPlayerTrapMaster();

        if (PhotonNetwork.IsMasterClient)
        {
            cameraTransform = mainPlayerCamerTransform;
        }
        else
            cameraTransform = runnerCameraTransform;
    }

    private void OnEnable()
    {
        if (view.OwnerActorNr > 0) Registry[view.OwnerActorNr] = this;
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.OnRunnerOut += HandleRunnerOut;
            RaceManager.Instance.OnRaceEnded += HandleRaceEnded;
        }
    }

    private void OnDisable()
    {
        if (view.OwnerActorNr > 0 && Registry.TryGetValue(view.OwnerActorNr, out var self) && self == this)
            Registry.Remove(view.OwnerActorNr);
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.OnRunnerOut -= HandleRunnerOut;
            RaceManager.Instance.OnRaceEnded -= HandleRaceEnded;
        }
    }

    private void HandleRunnerOut(int actorNumber, bool finished)
    {
        if (actorNumber != view.OwnerActorNr) return;

        isOut = true;
        if (view.IsMine) rb.linearVelocity = Vector3.zero;
    }

    private void HandleRaceEnded()
    {
        raceFullyEnded = true;
        if (view.IsMine) rb.linearVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (isOut || raceFullyEnded) return;

        if (view.IsMine)
        {
            UpdateGroundCheck();
            SimulateLocalMovement();
        }
        else InterpolateRemote();
    }

    private void UpdateGroundCheck()
    {
        if (groundCheck == null) return;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void SimulateLocalMovement()
    {
        if (isKnockedDown)
        {
            knockdownTimer -= Time.fixedDeltaTime;
            if (knockdownTimer <= 0f) isKnockedDown = false;
            return;
        }

        float speedMultiplier = GetGrabMultiplier();
        Vector3 direction = GetCameraRelativeDirection();

        Vector3 targetVel = direction * moveSpeed * speedMultiplier + windVelocity;
        targetVel.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVel;
        windVelocity = Vector3.MoveTowards(windVelocity, Vector3.zero, windDecay * Time.fixedDeltaTime);

        if (jumpRequested)
        {
            jumpRequested = false;

            if (isRunner && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
        }
    }

    private void InterpolateRemote()
    {
        rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * interpolationSpeed));
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.fixedDeltaTime * interpolationSpeed);
    }

    private Vector3 GetCameraRelativeDirection()
    {
        if (cameraTransform == null)
        {
            return (transform.forward * moveinput.y + transform.right * moveinput.x).normalized;
        }

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = camForward * moveinput.y + camRight * moveinput.x;
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
    }

    private float GetGrabMultiplier()
    {
        return (isBeingGrabbed || isGrabbingSomeone) ? grabSlowMultiplier : 1f;
    }

    public void OnMove(InputValue action)
    {
        moveinput = action.Get<Vector2>();
    }

    public void OnJump(InputValue action)
    {
        if (action.isPressed) jumpRequested = true;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        isKnockedDown = true;
        knockdownTimer = knockdownDuration;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    public void ApplyWind(Vector3 direction, float speed, float decay = 20f)
    {
        windVelocity = direction.normalized * speed;
        windDecay = decay;
    }

    public void SetAttemptingGrab(bool value)
    {
        isAttemptingGrab = value;
    }
    public void SetBeingGrabbed(bool value)
    {
        isBeingGrabbed = value;
    }

    public void SetGrabbingSomeone(bool value)
    {
        isGrabbingSomeone = value;
    }

    [PunRPC]
    public void RPC_ApplyPush(Vector3 direction, float force)
    {
        if (!photonView.IsMine) return;
        ApplyKnockback(direction, force);
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
    }

    public void RespawnAtCheckpoint()
    {
        isKnockedDown = false;
        rb.linearVelocity = Vector3.zero;
        rb.position = checkpointPosition;
        transform.position = checkpointPosition;

        networkPosition = checkpointPosition;
        networkRotation = transform.rotation;
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