using UnityEngine;

/// <summary>
///     プレイヤーの反撃用ホーミング弾の挙動を管理するクラス
/// </summary>
public class PlayerCounterBullet : HomingBulletBase
{
    protected override void HandleHit(Collider other)
    {
        if (other.transform == Shooter) return;

        if (other.TryGetComponent<EnemyBase>(out EnemyBase hit))
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
