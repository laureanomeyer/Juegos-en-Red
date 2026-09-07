using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

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

    private PlayerMovement movement;
    private float pushCooldownTimer;
    private float grabCooldownTimer;
    private bool isGrabbing;
    private PlayerCombat currentGrabTarget;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        if (pushCooldownTimer > 0f) pushCooldownTimer -= Time.deltaTime;
        if (grabCooldownTimer > 0f) grabCooldownTimer -= Time.deltaTime;

        if (Mouse.current.leftButton.wasPressedThisFrame && pushCooldownTimer <= 0)
        {
            TryPush();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && grabCooldownTimer <= 0)
        {
            TryStartGrab();
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

    private PlayerCombat FindTarget(float range, float radius)
    {
        Vector3 origin = transform.position + transform.forward * (range * 0.5f);
        Collider[] hits = Physics.OverlapSphere(origin, radius, playerLayer);

        PlayerCombat closest = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            var combat = hit.GetComponentInParent<PlayerCombat>();
            if (combat == null || combat == this) continue;

            float dist = Vector3.Distance(transform.position, combat.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = combat;
            }
        }
        return closest;
    }

    private void TryPush()
    {
        var target = FindTarget(pushRange, pushRadius);
        if (target == null) return;

        pushCooldownTimer = pushCooldown;
        Vector3 direction = (target.transform.position - transform.position).normalized;
        target.photonView.RPC(nameof(ApplyKnockback), RpcTarget.All, direction, pushForce);
    }

    private void TryStartGrab()
    {
        var target = FindTarget(grabRange, grabRadius);
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
    }
}