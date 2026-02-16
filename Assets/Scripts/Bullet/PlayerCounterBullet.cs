using UnityEngine;

/// <summary>
///     プレイヤーのカウンターホーミング弾クラス
/// </summary>
public class PlayerCounterBullet : HomingBulletBase
{
    protected override void HandleHit(Collider other)
    {
        if (other.transform == Shooter) return;

        if (other.TryGetComponent<EnemyBase>(out EnemyBase hit))
        {
            ProjectileService.Instance.SpawnExplosion(ExplosionType.Small, this.transform);
            hit.TakeDamage(_damage);
            Release();
            return;
        }

        if (other.CompareTag("Obstacle"))
        {
            Release();
        }
    }
}
