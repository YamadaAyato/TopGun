using UnityEngine;

public class EnemyBullet : BulletBase
{
    [SerializeField] private int _damage;

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
