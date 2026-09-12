using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MaceTrap : MonoBehaviour, ITrap
{
    [Header("Debug")]
    [SerializeField] private bool debugActivar;

    [Header("Péndulo")]
    [SerializeField] private Vector3 swingAxis = Vector3.forward;

    [SerializeField] private float swingAmplitude = 75f;
    [SerializeField] private float swingSpeed = 2f;
    [SerializeField] private float damping = 0.7f;

    [Header("Daño")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float hitCooldownPerTarget = 0.5f;

    private Quaternion restRotation;

    private bool isSwinging;
    private Coroutine swingRoutine;

    private readonly Dictionary<PlayerVitals, float> lastHitTime =
        new Dictionary<PlayerVitals, float>();


    private void Awake()
    {
        // Guardamos la rotación inicial de la maza
        restRotation = transform.localRotation;
    }


    // =========================================================
    // ESTA ES LA FUNCIÓN QUE LLAMA EL BOTÓN
    // =========================================================

    public void Activate()
    {
        // Si ya estaba oscilando, reiniciamos el movimiento
        if (swingRoutine != null)
        {
            StopCoroutine(swingRoutine);
        }

        swingRoutine = StartCoroutine(SwingRoutine());
    }


    // =========================================================
    // MOVIMIENTO DE PÉNDULO
    // =========================================================

    private IEnumerator SwingRoutine()
    {
        isSwinging = true;

        float duration = 1.5f;
        float time = 0f;

        // Comienza en la posición de reposo (+90°)
        transform.localRotation = restRotation;

        // Primera mitad: +90° -> -90°
        while (time < duration / 2f)
        {
            time += Time.deltaTime;

            float t = time / (duration / 2f);

            // Suavizado
            t = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.Lerp(0f, -180f, t);

            transform.localRotation =
                restRotation *
                Quaternion.AngleAxis(angle, swingAxis);

            yield return null;
        }

        // Segunda mitad: -90° -> +90°
        time = 0f;

        while (time < duration / 2f)
        {
            time += Time.deltaTime;

            float t = time / (duration / 2f);

            // Suavizado
            t = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.Lerp(-180f, 0f, t);

            transform.localRotation =
                restRotation *
                Quaternion.AngleAxis(angle, swingAxis);

            yield return null;
        }

        // Aseguramos que termine exactamente en el reposo
        transform.localRotation = restRotation;

        isSwinging = false;
        swingRoutine = null;
    }


    // =========================================================
    // DAÑO
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }


    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }


    private void TryDamage(Collider other)
    {
        if (!isSwinging)
            return;

        PlayerVitals vitals =
            other.GetComponentInParent<PlayerVitals>();

        if (vitals == null)
            return;

        if (lastHitTime.TryGetValue(vitals, out float last))
        {
            if (Time.time - last < hitCooldownPerTarget)
                return;
        }

        lastHitTime[vitals] = Time.time;

        vitals.photonView.RPC(
            nameof(PlayerVitals.ApplyDamage),
            RpcTarget.All,
            damage
        );
    }


    // =========================================================
    // DEBUG
    // =========================================================

    private void OnValidate()
    {
        if (debugActivar)
        {
            debugActivar = false;

            if (Application.isPlaying)
            {
                Activate();
            }
        }
    }
}