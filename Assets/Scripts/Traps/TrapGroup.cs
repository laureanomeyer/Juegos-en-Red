using System.Collections.Generic;
using UnityEngine;

public class TrapGroup : MonoBehaviour, ITrap
{
    private ITrap[] traps;

    private void Awake()
    {
        var found = new List<ITrap>();

        foreach (var behaviour in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour == this) continue; // no incluirse a sí mismo
            if (behaviour is ITrap trap) found.Add(trap);
        }

        traps = found.ToArray();
    }

    public void Activate()
    {
        foreach (var trap in traps)
        {
            trap.Activate();
        }
    }
}