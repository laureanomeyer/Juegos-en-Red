using Photon.Pun;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class PlayerCombat : MonoBehaviourPun
{
    [SerializeField] private float pushRange = 1.5f;
    [SerializeField] private float pushRadius = 0.6f;
    [SerializeField] private float pushForce = 8f;
    [SerializeField] private float pushCooldown = 1f;

    [SerializeField] private float grabRange = 2f;
    [SerializeField] private float grabRadius = 0.8f;
    [SerializeField] private float grabCooldown = 2f;

    [SerializeField] private LayerMask playerLayer;

    Vector3 origin;
    float radius;

    private PlayerMovement movement;
    private float pushCooldownTimer;
    private float grabCooldownTimer;
    private bool isGrabbing;
    private PlayerCombat currentGrabTarget;

    private bool raceEnded;
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
            Debug.Log("pushe");
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && grabCooldownTimer <= 0)
        {
            TryStartGrab();
            Debug.Log("agarre");
        }

        else if (Mouse.current.rightButton.wasReleasedThisFrame && isGrabbing)
        {
            EndGrab();
        }

        if (isGrabbing && currentGrabTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentGrabTarget.transform.position);
            if ( dist > grabRange * 1.3f) 
            {
                EndGrab();
            }
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

    private Component FindPushTarget(float radius)
    {
        Vector3 origin = transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, radius, playerLayer);

        this.origin = origin;
        this.radius = radius;

        Component closest = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var combat = hit.GetComponentInParent<PlayerCombat>();
            if (combat != null && combat != this)
            {
                float dist = Vector3.Distance(transform.position, combat.transform.position);
                if (dist < closestDist) { closestDist = dist; closest = combat; }
                continue;
            }

            var boton = hit.GetComponentInParent<ButtonBehavior>();
            if (boton != null)
            {
                float dist = Vector3.Distance(transform.position, boton.transform.position);
                if (dist < closestDist) { closestDist = dist; closest = boton; }
            }
        }
        return closest;
    }

    private void TryPush()
    {
        var target = FindPushTarget(pushRadius);
        if (target == null) return;

        pushCooldownTimer = pushCooldown;

        if (target is PlayerCombat playerTarget)
        {
            Vector3 direction = (playerTarget.transform.position - transform.position).normalized;
            playerTarget.photonView.RPC(nameof(ApplyKnockback), RpcTarget.All, direction, pushForce);
        }
        else if (target is ButtonBehavior boton)
        {
            boton.Interact(this);
        }
    }

    private void TryStartGrab()
    {
        if (isGrabbing) return;

        var target = FindPlayerTarget(grabRadius); 
        if (target == null) return;

        currentGrabTarget = target;
        isGrabbing = true;
        movement.SetGrabPartner(target.photonView.OwnerActorNr);
        target.photonView.RPC(nameof(ReceiveGrab), RpcTarget.All, photonView.OwnerActorNr);
    }

    private void EndGrab()
    {
        if ( currentGrabTarget != null)
        {
            currentGrabTarget.photonView.RPC(nameof(ReceiveGrab), RpcTarget.All, -1);

            currentGrabTarget = null;
            isGrabbing = false;
            grabCooldownTimer = grabCooldown;
            movement.SetGrabPartner(-1);
        }
    }
    [PunRPC]
    private void ApplyKnockback(Vector3 direction, float force)
    {
        if (!photonView.IsMine) return;

        movement.ApplyKnockback(direction, force);
    }

    [PunRPC]
    private void ReceiveGrab(int grabberActorNumber)
    {
        if (!photonView.IsMine) return;
   
        movement.SetGrabPartner(grabberActorNumber);
        Debug.Log("Recibi agarre");
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(origin, radius);
    }
}