// HandsHolder.cs (en el jugador, con un hijo "HoldPoint" frente a la cámara, algo bajo)
using UnityEngine;

public class HandsHolder : MonoBehaviour
{
    [SerializeField] Transform holdPoint;
    [SerializeField] Collider[] playerColliders;   // para ignorar colisión con el objeto agarrado
    [SerializeField] float followStrength = 25f;
    [SerializeField] float maxSpeed = 15f;
    [SerializeField] float rotationStrength = 15f;
    [SerializeField] float breakDistance = 2.5f;
    [SerializeField] float minSpeedMultiplier = 0.5f;
    [SerializeField] float arriveDistance = 0.6f;   // a esta distancia se considera "llegó a la mano"
    [SerializeField] float arriveTimeout = 2f;      // si no llega en este tiempo, se suelta

    public bool IsHolding => _held != null;
    public IPickable Held => _held;

    bool _arrived;
    float _pickupTime;

    // Multiplicador para tu script de movimiento: speed *= hands.SpeedMultiplier
    public float SpeedMultiplier =>
        _held == null ? 1f : Mathf.Lerp(1f, minSpeedMultiplier, _held.Weight);

    IPickable _held;
    Rigidbody _rb;

    public void PickUp(IPickable pickable)
    {
        if (IsHolding) return;
        _held = pickable;
        _rb = pickable.Body;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.WakeUp();
        _arrived = false;
        _pickupTime = Time.time;

        foreach (var pc in playerColliders)
            foreach (var c in _rb.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(pc, c, true);

        _held.OnPickedUp(this);
    }

    public void Drop()
    {
        if (!IsHolding) return;

        foreach (var pc in playerColliders)
            foreach (var c in _rb.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(pc, c, false);

        _held.OnDropped(this);
        _held = null;
        _rb = null;
    }

    void FixedUpdate()
    {
        if (!IsHolding) return;

        Vector3 toTarget = holdPoint.position - _rb.position;
        float dist = toTarget.magnitude;

        if (!_arrived && dist < arriveDistance) _arrived = true;

        // Una vez que llegó, se suelta si algo lo aleja; si nunca llega (trabado detrás de algo), también
        if (_arrived && dist > breakDistance) { Drop(); return; }
        if (!_arrived && Time.time - _pickupTime > arriveTimeout) { Drop(); return; }

        // Más peso = responde más lento = se siente pesado y se arrastra
        float heavy = Mathf.Lerp(1f, 0.25f, _held.Weight);

        Vector3 desiredVel = toTarget * followStrength * heavy;
        _rb.linearVelocity = Vector3.ClampMagnitude(desiredVel, maxSpeed);

        // Rotación suave hacia la orientación de la mano
        Quaternion delta = holdPoint.rotation * Quaternion.Inverse(_rb.rotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        if (Mathf.Abs(angle) > 0.01f && axis.sqrMagnitude > 0.001f)
            _rb.angularVelocity = axis.normalized * (angle * Mathf.Deg2Rad * rotationStrength * heavy);
    }
}