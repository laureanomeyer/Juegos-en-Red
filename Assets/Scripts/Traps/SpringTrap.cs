using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpringTrap : MonoBehaviour, ITrap
{
    [Header("Movimiento del resorte")]
    [SerializeField] private Vector3 extendedLocalOffset = new Vector3(0f, 0f, 1f);
    [SerializeField] private float extendDuration = 0.12f;
    [SerializeField] private float holdDuration = 0.2f;
    [SerializeField] private float retractDuration = 0.3f;

    [Header("Empuje")]
    [SerializeField] private Transform pushDirectionSource;
    [SerializeField] private float pushForce = 20f;
    [SerializeField] private float hitCooldownPerTarget = 0.5f;

    private Vector3 restLocalPos;
    private bool isExtended;
    private Coroutine moveRoutine;

    private readonly Dictionary<PlayerMovement, float> lastHitTime = new Dictionary<PlayerMovement, float>();

    private void Awake()
    {
        restLocalPos = transform.localPosition;

        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[SpringTrap] '{name}': el Collider no tiene 'Is Trigger' tildado, el empuje no va a detectar contacto.", this);
        }
    }

    public void Activate()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        yield return MoverA(restLocalPos + extendedLocalOffset, extendDuration);

        isExtended = true;
        yield return new WaitForSeconds(holdDuration);
        isExtended = false;

        yield return MoverA(restLocalPos, retractDuration);
        moveRoutine = null;
    }

    private IEnumerator MoverA(Vector3 destinoLocal, float duracion)
    {
        Vector3 desde = transform.localPosition;
        float t = 0f;

        while (t < duracion)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(desde, destinoLocal, Mathf.Clamp01(t / duracion));
            yield return null;
        }

        transform.localPosition = destinoLocal;
    }

    private void OnTriggerEnter(Collider other) => TryPush(other);
    private void OnTriggerStay(Collider other) => TryPush(other);

    private void TryPush(Collider other)
    {
        if (!isExtended) return;

        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();
        if (movement == null) return;

        PhotonView view = movement.GetComponent<PhotonView>();
        if (view == null || !view.IsMine) return; // solo el dueño de ESE jugador reporta su propio empujón

        if (lastHitTime.TryGetValue(movement, out float last) && Time.time - last < hitCooldownPerTarget)
            return;

        lastHitTime[movement] = Time.time;

        Vector3 direction = (pushDirectionSource != null ? pushDirectionSource.forward : transform.forward).normalized;
        view.RPC(nameof(PlayerMovement.RPC_ApplyPush), RpcTarget.All, direction, pushForce);
    }

    private void OnValidate()
    {
    }
}