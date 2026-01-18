using System;
using UnityEngine;

public static class GameEvents
{
    public static Action<float> OnRegenerationEvent;
    public static Action<int> OnBloodScoreChanged;

        public static void InvokeBloodScoreChanged(int amount)
    {
        OnBloodScoreChanged?.Invoke(amount);
    }
}

