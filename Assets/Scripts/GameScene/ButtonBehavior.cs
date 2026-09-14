using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    [Header("Trampa que controla este botón")]
    [SerializeField] private MonoBehaviour trapBehaviour;
    private ZoneLevelBuilder builder;
    private int zoneIndex;

    public ITrap Trap => trapBehaviour as ITrap;

    private void Awake()
    {
        
    }

    public void Initialize(ZoneLevelBuilder builder, int zoneIndex)
    {
        this.builder = builder;
        this.zoneIndex = zoneIndex;
    }

    public void Interact(PlayerCombat pusher)
    {
        builder.RequestTriggerTrap(zoneIndex);
    }

    private void OnValidate()
    {

    }
}