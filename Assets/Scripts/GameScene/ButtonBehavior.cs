using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    private ZoneLevelBuilder builder;
    private int zoneIndex;

    // Llamado por ZoneLevelBuilder justo después de instanciar la zona.
    public void Initialize(ZoneLevelBuilder builder, int zoneIndex)
    {
        this.builder = builder;
        this.zoneIndex = zoneIndex;
    }

    public void Interact(PlayerCombat pusher)
    {
        builder.RequestTriggerTrap(zoneIndex);
    }
}
