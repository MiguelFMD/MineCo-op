using System;

public static class EventManager
{
    public static Action<ulong> OnPlayerDied;
    public static Action<ulong, float> OnHealthChanged;
}