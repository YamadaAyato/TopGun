using UnityEngine;

/// <summary>
///     敵のホーミング弾クラス
/// </summary>
public class EnemyHomingBullet : HomingBulletBase
{
    protected override void HandleHit(Collider other)
    {
        if (other.transform == Shooter) return;

        if (other.TryGetComponent<IDamageable>(out IDamageable hit))
        {
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
