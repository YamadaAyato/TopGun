using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<float> OnPlayerHit;

    public static event Action<Vector3> OnExplosion;

    public static void RaisePlayerHit(float intensity)
    {
        OnPlayerHit?.Invoke(intensity);
    }

    public static void RaiseExplosion(Vector3 position)
    {
        OnExplosion?.Invoke(position);
    }
}
