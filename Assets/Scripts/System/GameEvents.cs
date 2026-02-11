using System;
using UnityEngine;

/// <summary>
///     Ingameの各種イベントを管理する静的クラス
/// </summary>
public static class GameEvents
{
    /// <summary>プレイヤーが被弾した時のイベント </summary>
    public static event Action<float> OnPlayerHit;

    /// <summary> 爆発が発生したときのイベント </summary>
    public static event Action<Vector3> OnExplosion;

    /// <summary> プレイヤーが回避行動をしたときのイベント </summary>
    public static event Action OnEvade;

    /// <summary>
    ///     プレイヤーが被弾したときに発火する
    /// </summary>
    /// <param name="intensity"></param>
    public static void RaisePlayerHit(float intensity)
    {
        OnPlayerHit?.Invoke(intensity);
    }

    /// <summary>
    ///     爆発が発生したときに発火する
    /// </summary>
    /// <param name="position"></param>
    public static void RaiseExplosion(Vector3 position)
    {
        OnExplosion?.Invoke(position);
    }

    /// <summary>
    ///     回避行動が発生したときに発火する
    /// </summary>
    public static void RaiseEvade()
    {
        OnEvade?.Invoke();
    }
}
