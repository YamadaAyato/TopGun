using UnityEngine;

/// <summary>
///     敵の銃弾処理をするクラス
/// </summary>
public class EnemyBullet : BulletBase
{
    protected override void HandleHit(Collider other)
    {
        if(other.TryGetComponent<PlayerHealth>(out PlayerHealth hit))
        {
            if (hit.CanBeHit)
            {
                hit.TakeDamage(_damage);
            }

            Release();
            return;
        }

        if(other.CompareTag("Obstacle"))
        {
            Release();
        }
    }
}
