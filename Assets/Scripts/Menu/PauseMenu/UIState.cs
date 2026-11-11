using System;
using UnityEngine;

public static class UIState
{
    private static int openCount;
    public static event Action<bool> OnChanged;
    public static bool IsAnyOpen => openCount > 0;
    public static int LastCloseFrame { get; private set; } = -1;

    public static void Open()
    {
        openCount++;
        if (openCount == 1) OnChanged?.Invoke(true);
    }

    public static void Close()
    {
        if (openCount == 0) return;

        openCount--;
        LastCloseFrame = Time.frameCount;
        if (openCount == 0) OnChanged?.Invoke(false);
    }

    // Por si el editor tiene desactivado el reinicio de dominio.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        openCount = 0;
        LastCloseFrame = -1;
        OnChanged = null;
    }
}