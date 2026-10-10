using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     有効な敵弾を方向表示へ知らせるクラス
/// </summary>
public class EnemyProjectileSignal : MonoBehaviour
{
    /// <summary> 方向表示の対象となる有効な敵弾。 </summary>
    public static IReadOnlyCollection<EnemyProjectileSignal> Active => _active;
    /// <summary> ミサイル用の表示を使用するかどうか。 </summary>
    public bool IsMissile => _isMissile;

    [SerializeField, Tooltip("ミサイル用の色と近距離点滅を使用する")] private bool _isMissile;

    private static readonly HashSet<EnemyProjectileSignal> _active = new HashSet<EnemyProjectileSignal>();

    private void OnEnable() => _active.Add(this);
    private void OnDisable() => _active.Remove(this);
    private void OnDestroy() => _active.Remove(this);
}
