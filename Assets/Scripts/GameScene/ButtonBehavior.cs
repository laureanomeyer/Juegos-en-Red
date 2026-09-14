using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    [Header("Trampa que controla ESTE botón")]
    [Tooltip("Arrastrá acá el objeto que tiene el script de la trampa (MaceTrap, SpringTrap, FanTrap, TrapGroup, etc). Tiene que implementar ITrap.")]
    [SerializeField] private MonoBehaviour trapBehaviour;

    private ZoneLevelBuilder builder;
    private int zoneIndex;

    public ITrap Trap => trapBehaviour as ITrap;

    private void Awake()
    {
        Debug.Log($"[Botón] lossyScale: {transform.lossyScale}");
    }
    // Llamado por ZoneLevelBuilder justo después de instanciar la zona.
    public void Initialize(ZoneLevelBuilder builder, int zoneIndex)
    {
        this.builder = builder;
        this.zoneIndex = zoneIndex;
    }

    public void Interact(PlayerCombat pusher)
    {
        if (builder == null)
        {
            Debug.LogWarning($"[ButtonBehavior] '{name}' no fue inicializado por ningún ZoneLevelBuilder (¿está fuera de una zona válida?).", this);
            return;
        }

        builder.RequestTriggerTrap(zoneIndex);
    }

    private void OnValidate()
    {
        if (trapBehaviour != null && !(trapBehaviour is ITrap))
        {
            Debug.LogWarning($"[ButtonBehavior] '{name}': el objeto asignado en 'Trap Behaviour' no implementa ITrap. Este botón no va a activar nada.", this);
        }
    }
}