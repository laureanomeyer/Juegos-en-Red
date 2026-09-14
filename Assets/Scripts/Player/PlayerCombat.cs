using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviourPun
{
    [Header("Empuje (jugador a jugador)")]
    [SerializeField] private float pushRadius = 2f;
    [SerializeField] private float pushForce = 8f;
    [SerializeField] private float pushCooldown = 1f;

    [Header("Agarre")]
    [SerializeField] private float grabRadius = 2f;
    [SerializeField] private float grabCooldown = 2f;

    [Header("Interacción con botones (tecla E)")]
    [SerializeField] private float interactRadius = 0.9f;
    [SerializeField] private LayerMask buttonLayer;
    [SerializeField] private LayerMask playerLayer;

    Vector3 origin;
    float radius;

    private PlayerMovement movement;
    private float pushCooldownTimer;
    private float grabCooldownTimer;
    private bool isHoldingGrabInput;
    private PlayerCombat currentGrabTarget;

    private bool isOut;
    private bool raceFullyEnded;

    public float PushCooldownRatio => pushCooldown <= 0f ? 0f : Mathf.Clamp01(pushCooldownTimer / pushCooldown);
    public float GrabCooldownRatio => grabCooldown <= 0f ? 0f : Mathf.Clamp01(grabCooldownTimer / grabCooldown);

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!photonView.IsMine || isOut || raceFullyEnded) return;

        if (pushCooldownTimer > 0f) pushCooldownTimer -= Time.deltaTime;
        if (grabCooldownTimer > 0f) grabCooldownTimer -= Time.deltaTime;

        if (Mouse.current.leftButton.wasPressedThisFrame && pushCooldownTimer <= 0)
        {
            TryPush();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && grabCooldownTimer <= 0)
        {
            StartGrabbing();
        }

        if (isHoldingGrabInput)
        {
            UpdateGrab();
        }

        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            StopGrabbing();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteractButton();
        }
    }

    private void OnEnable()
    {
        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.OnRunnerOut += HandleRunnerOut;
            RaceManager.Instance.OnRaceEnded += () => raceFullyEnded = true;
        }
    }
    private void OnDisable()
    {
        if (RaceManager.Instance != null) RaceManager.Instance.OnRunnerOut -= HandleRunnerOut;
    }
    private void HandleRunnerOut(int actorNumber, bool finished)
    {
        if (actorNumber == photonView.OwnerActorNr) isOut = true;
    }

    private PlayerCombat FindPlayerTarget(float radius)
    {
        Vector3 origin = transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, radius, playerLayer);

        this.origin = origin;
        this.radius = radius;

        PlayerCombat closest = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var combat = hit.GetComponentInParent<PlayerCombat>();
            if (combat == null || combat == this) continue;

            float dist = Vector3.Distance(transform.position, combat.transform.position);
            if (dist < closestDist) { closestDist = dist; closest = combat; }
        }
        return closest;
    }

    private ButtonBehavior FindButtonTarget(float radius)
    {
        Vector3 origin = transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, radius, buttonLayer);

        this.origin = origin;
        this.radius = radius;

        ButtonBehavior closest = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var boton = hit.GetComponentInParent<ButtonBehavior>();
            if (boton == null) continue;

            float dist = Vector3.Distance(transform.position, boton.transform.position);
            if (dist < closestDist) { closestDist = dist; closest = boton; }
        }
        return closest;
    }

    private void TryPush()
    {
        pushCooldownTimer = pushCooldown;

        var target = FindPlayerTarget(pushRadius);
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        target.photonView.RPC(nameof(ApplyKnockback), RpcTarget.All, direction, pushForce);
    }

    private void TryInteractButton()
    {
        var boton = FindButtonTarget(interactRadius);
        if (boton == null) return;

        boton.Interact(this);
    }

    private void StartGrabbing()
    {
        isHoldingGrabInput = true;
        movement.SetGrabbingSomeone(true);
    }

    private void UpdateGrab()
    {
        if (currentGrabTarget == null)
        {
            var target = FindPlayerTarget(grabRadius);
            if (target != null)
            {
                currentGrabTarget = target;
                target.photonView.RPC(nameof(ReceiveGrabState), RpcTarget.All, true);
            }
        }
        else
        {
            float dist = Vector3.Distance(transform.position, currentGrabTarget.transform.position);
            if (dist > grabRadius * 5f)
            {
                ReleaseCurrentTarget();
            }
        }
    }

    private void StopGrabbing()
    {
        if (!isHoldingGrabInput) return;

        isHoldingGrabInput = false;
        movement.SetGrabbingSomeone(false);
        grabCooldownTimer = grabCooldown;
        ReleaseCurrentTarget();
    }

    private void ReleaseCurrentTarget()
    {
        if (currentGrabTarget != null)
        {
            currentGrabTarget.photonView.RPC(nameof(ReceiveGrabState), RpcTarget.All, false);
            currentGrabTarget = null;
        }
    }

    [PunRPC]
    private void ApplyKnockback(Vector3 direction, float force)
    {
        if (!photonView.IsMine) return;

        movement.ApplyKnockback(direction, force);
    }

    [PunRPC]
    private void ReceiveGrabState(bool grabbed)
    {
        if (!photonView.IsMine) return;
        movement.SetBeingGrabbed(grabbed);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(origin, radius);
    }
}