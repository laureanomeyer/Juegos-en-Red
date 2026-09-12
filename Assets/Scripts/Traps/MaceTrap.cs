using System.Collections;
using Photon.Pun;
using UnityEngine;

public class MaceTrap : MonoBehaviour, ITrap
{

    [Header("Debug")]
    [SerializeField] private bool debugActivar;

    [Header("Péndulo")]
    [SerializeField] private Vector3 swingAxis = Vector3.forward;

    [SerializeField] private float swingDuration = 1.5f;

    [Header("Daño")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float hitCooldownPerTarget = 0.5f;

    private Quaternion restRotation;

    private bool isSwinging;
    private Coroutine swingRoutine;

    private readonly System.Collections.Generic.Dictionary<PlayerVitals, float> lastHitTime =
        new System.Collections.Generic.Dictionary<PlayerVitals, float>();


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

    private void OnValidate()
    {
        if (debugActivar)
        {
            debugActivar = false; // se destilda solo, para poder volver a probar
            if (Application.isPlaying) Activate();
        }
    }
}