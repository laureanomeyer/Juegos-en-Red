using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovingBlockTrap : MonoBehaviour, ITrap
{
    [Header("Debug")]
    [SerializeField] private bool debugActivar;

    [Header("Movimiento")]
    [Tooltip("Desplazamiento LOCAL (según la rotación del bloque) desde la posición de reposo hasta la posición extendida.")]
    [SerializeField] private Vector3 extendedLocalOffset = new Vector3(0f, 0f, 3f);
    [SerializeField] private float extendDuration = 0.6f;
    [Tooltip("Cuánto se queda extendido antes de volver.")]
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
        rb.isKinematic = true; // clave: kinematic + MovePosition = empuja por física sin que la física lo mueva a él

        var col = GetComponent<Collider>();
        if (col.isTrigger)
        {
            Debug.LogWarning($"[MovingBlockTrap] '{name}': el Collider está marcado como 'Is Trigger'. Tiene que ser SÓLIDO para poder empujar por contacto físico.", this);
        }

        restPos = rb.position;
        // Convierte el offset local a una dirección mundial según la rotación
        // actual del bloque (no aplica escala, es una dirección pura).
        worldOffset = transform.TransformDirection(extendedLocalOffset);
    }

    public void Activate()
    {
        if (estado != Estado.Reposo) return; // ya está en movimiento, no lo reinicia a mitad de camino
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
        if (debugActivar)
        {
            debugActivar = false; // se destilda solo, para poder volver a probar
            if (Application.isPlaying) Activate();
        }
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