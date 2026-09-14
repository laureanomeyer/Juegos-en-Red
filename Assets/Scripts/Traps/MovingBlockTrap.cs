using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovingBlockTrap : MonoBehaviour, ITrap
{
    [Header("Debug")]
    [SerializeField] private bool debugActivar;
    [Header("Movimiento")]
    [SerializeField] private Vector3 extendedLocalOffset = new Vector3(0f, 0f, 3f);
    [SerializeField] private float extendDuration = 0.6f;
    [SerializeField] private float holdDuration = 0.3f;
    [SerializeField] private float retractDuration = 0.6f;

    private Rigidbody rb;
    private Vector3 restPos;
    private Vector3 worldOffset;
    private enum Estado { Reposo, Extendiendo, Sosteniendo, Retrayendo }
    private Estado estado = Estado.Reposo;
    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; 

        var col = GetComponent<Collider>();
        restPos = rb.position;
        worldOffset = transform.TransformDirection(extendedLocalOffset);
    }

    public void Activate()
    {
        if (estado != Estado.Reposo) return;
        estado = Estado.Extendiendo;
        timer = 0f;
    }

    private void FixedUpdate()
    {
        switch (estado)
        {
            case Estado.Extendiendo:
                timer += Time.fixedDeltaTime;
                float tExt = Mathf.Clamp01(timer / extendDuration);
                rb.MovePosition(Vector3.Lerp(restPos, restPos + worldOffset, tExt));
                if (tExt >= 1f) { estado = Estado.Sosteniendo; timer = 0f; }
                break;

            case Estado.Sosteniendo:
                timer += Time.fixedDeltaTime;
                if (timer >= holdDuration) { estado = Estado.Retrayendo; timer = 0f; }
                break;

            case Estado.Retrayendo:
                timer += Time.fixedDeltaTime;
                float tRet = Mathf.Clamp01(timer / retractDuration);
                rb.MovePosition(Vector3.Lerp(restPos + worldOffset, restPos, tRet));
                if (tRet >= 1f) { estado = Estado.Reposo; timer = 0f; }
                break;
        }
    }

    private void OnValidate()
    {
    }

    private void OnDrawGizmosSelected()
    {

        Vector3 origen = Application.isPlaying ? restPos : transform.position;
        Vector3 offset = Application.isPlaying ? worldOffset : transform.TransformDirection(extendedLocalOffset);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(origen, origen + offset);
        Gizmos.DrawWireSphere(origen + offset, 0.2f);
    }
}