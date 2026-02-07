using UnityEngine;

/// <summary>
///     敵の射撃タイプ
/// </summary>
public enum EnemyShooterType
{
    Straight,
    Homing,
    Random
}

/// <summary>
///     敵の射撃管理をするクラス
/// </summary>
public class EnemyShooter : MonoBehaviour, IEnemyShooter
{
    [Header("参照")]
    [SerializeField] private Transform _muzzle;

    [Header("射撃設定")]
    [SerializeField] private EnemyShooterType _shooterType;
    [SerializeField, Range(0f, 1f)] private float _homingRate;

    /// <summary>
    ///     発射!!!!!!!!!
    /// </summary>
    /// <param name="shooter"></param>
    /// <param name="target"></param>
    public void Fire(Transform shooter, Transform target)
    {
        // nullチェック
        if(_muzzle == null || shooter == null || target == null)
        {
            Debug.LogWarning("EnemyShooter: Muzzle, Shooter, or Target is null.");
            return;
        }
        if(ProjectileService.Instance == null)
        {
            Debug.LogWarning("EnemyShooter: ProjectileService instance is null.");
            return;
        }

        // 射撃タイプによって使い分けする
        bool useHoming =_shooterType switch
        {
            EnemyShooterType.Straight => false,
            EnemyShooterType.Homing => true,
            _ => Random.value < _homingRate,
        };

        // 射撃へつなぐ
        if(useHoming)
        {
            ProjectileService.Instance.SpawnHomingBullet(_muzzle, shooter, target);
        }
        else
        {
            Vector3 dir = (target.position - _muzzle.position).normalized;
            ProjectileService.Instance.SpawnStraightBullet(_muzzle, shooter, dir);
        }
    }

    public void FireStraight(Transform shooter, Vector3 dir)
    {
        if (_muzzle == null || shooter == null) return;
        if (ProjectileService.Instance == null) return;

        ProjectileService.Instance.SpawnStraightBullet(_muzzle, shooter, dir);
    }
}
