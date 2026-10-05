using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     有効な敵弾を方向表示へ知らせるクラス
/// </summary>
public class EnemyProjectileSignal : MonoBehaviour
{
    public static readonly HashSet<EnemyProjectileSignal> Active = new HashSet<EnemyProjectileSignal>();
    public bool IsMissile => _isMissile;
    [SerializeField] private bool _isMissile;

    private void OnEnable() => Active.Add(this);
    private void OnDisable() => Active.Remove(this);
    private void OnDestroy() => Active.Remove(this);
}

