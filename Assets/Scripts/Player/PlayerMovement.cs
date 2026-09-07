using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float knockdownDuration = 1f;
    [SerializeField] private float grabSlowRadius = 2f;    
    [SerializeField] private float grabMaxMultiplier = 0.5f; 
    [SerializeField] private float grabMinMultiplier = 0.2f; 

    private Rigidbody rb;
    private Vector2 moveinput;
    private PhotonView view;

    private bool isKnockedDown;
    private float knockdownTimer;
    private int grabPartnerActor = -1;

    public static readonly Dictionary<int, PlayerMovement> Registry = new Dictionary<int, PlayerMovement>();

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnEnable()
    {
        if (view.OwnerActorNr > 0) Registry[view.OwnerActorNr] = this;
    }

    private void OnDisable()
    {
        if (view.OwnerActorNr > 0 && Registry.TryGetValue(view.OwnerActorNr, out var self) && self == this)
            Registry.Remove(view.OwnerActorNr);
    }

    private void FixedUpdate()
    {
        if (!view.IsMine) return;

        if (isKnockedDown)
        {
            knockdownTimer -= Time.fixedDeltaTime;
            if (knockdownTimer <= 0f) isKnockedDown = false;
            return; 
        }

        float speedMultiplier = GetGrabMultiplier();
        Vector3 direction = (transform.forward * moveinput.y + transform.right * moveinput.x).normalized;
        Vector3 move = direction * moveSpeed * speedMultiplier * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    private float GetGrabMultiplier()
    {
        if (grabPartnerActor < 0) return 1f;
        if (!Registry.TryGetValue(grabPartnerActor, out var partner) || partner == null) return 1f;

        float dist = Vector3.Distance(transform.position, partner.transform.position);
        float t = Mathf.Clamp01(dist / grabSlowRadius);
        return Mathf.Lerp(grabMinMultiplier, grabMaxMultiplier, t);
    }

    public void OnMove(InputValue action)
    {
        moveinput = action.Get<Vector2>();
    }

    public void SetGrabPartner(int actorNumber)
    {
        grabPartnerActor = actorNumber;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        isKnockedDown = true;
        knockdownTimer = knockdownDuration;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.VelocityChange);
    }
}