using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MaceTrap : MonoBehaviour, ITrap
{

    [Header("Debug")]
    [SerializeField] private bool debugActivar;

    [Header("Péndulo")]
    [SerializeField] private Vector3 swingAxis = Vector3.forward;

    [SerializeField] private float swingDuration = 1.5f;

    [Header("Empuje")]
    [SerializeField] private float pushForce = 12f;
    [SerializeField] private float hitCooldownPerTarget = 0.5f;

    private Quaternion restRotation;

    private bool isSwinging;
    private int swingDirectionSign = -1; // hacia qué lado está yendo la maza en este instante del swing
    private Coroutine swingRoutine;

    private readonly Dictionary<PlayerMovement, float> lastHitTime = new Dictionary<PlayerMovement, float>();


    private void Awake()
    {
        restRotation = transform.localRotation;
    }



    public void Activate()
    {
        if (swingRoutine != null)
        {
            StopCoroutine(swingRoutine);
        }

        swingRoutine = StartCoroutine(SwingRoutine());
    }




    private IEnumerator SwingRoutine()
    {
        isSwinging = true;

        float halfDuration = swingDuration / 2f;
        float time = 0f;

        swingDirectionSign = -1; // primera mitad: 0 -> -180

        while (time < halfDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / halfDuration);


            t = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.Lerp(0f, -180f, t);

            transform.localRotation =
                restRotation *
                Quaternion.AngleAxis(angle, swingAxis);

            yield return null;
        }


        time = 0f;
        swingDirectionSign = 1; // segunda mitad: -180 -> 0 (vuelve)

        while (time < halfDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / halfDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            float angle = Mathf.Lerp(-180f, 0f, t);

            transform.localRotation =
                restRotation *
                Quaternion.AngleAxis(angle, swingAxis);

            yield return null;
        }


        transform.localRotation = restRotation;

        isSwinging = false;
        swingRoutine = null;
    }



    public void TryDamage(Collider other)
    {
        if (!isSwinging)
            return;

        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();
        if (movement == null)
            return;

        PhotonView view = movement.GetComponent<PhotonView>();
        if (view == null || !view.IsMine) return; // solo el dueño de ESE jugador reporta su propio empujón

        if (lastHitTime.TryGetValue(movement, out float last))
        {
            if (Time.time - last < hitCooldownPerTarget)
                return;
        }

        lastHitTime[movement] = Time.time;

        // Dirección tangencial al swing: perpendicular al eje de giro y al
        // brazo (pivote -> jugador), con el signo de qué lado está yendo la
        // maza en este instante.
        Vector3 worldAxis = transform.TransformDirection(swingAxis).normalized;
        Vector3 pivotToTarget = other.transform.position - transform.position;
        Vector3 tangent = Vector3.Cross(worldAxis, pivotToTarget);

        if (tangent.sqrMagnitude < 0.0001f) tangent = transform.right; // caso raro: target casi sobre el eje

        Vector3 pushDirection = tangent.normalized * swingDirectionSign;

        view.RPC(nameof(PlayerMovement.RPC_ApplyPush), RpcTarget.All, pushDirection, pushForce);
    }

    private void OnValidate()
    {
        if (debugActivar)
        {
            debugActivar = false; // se destilda solo, para poder volver a probar
            if (Application.isPlaying) Activate();
        }
    }
}