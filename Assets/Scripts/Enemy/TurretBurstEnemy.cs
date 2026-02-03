using System.Collections;
using UnityEngine;

public class TurretBurstEnemy : TurretEnemyBase
{
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private int _burstCount;
    [SerializeField] private float _burstInterval;
    [SerializeField] private int _bulletPoolSize;
    [SerializeField] private float _spreadAngle;

    private ObjectPool<EnemyBullet> _bulletPool;
    private Coroutine _burstCoroutine;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        _bulletPool = new ObjectPool<EnemyBullet>(_bulletPrefab, this.transform, _bulletPoolSize);
    }

    protected override void FireAtPlayer(Transform player)
    {
        if (_burstCoroutine == null)
        {
            _burstCoroutine = StartCoroutine(BurstFireCoroutine(player));
        }
    }

    private IEnumerator BurstFireCoroutine(Transform player)
    {
        for (int i = 0; i < _burstCount; i++)
        {
            ShootOnce(player);
            yield return new WaitForSeconds(_burstInterval);
        }
        _burstCoroutine = null;
    }

    private void ShootOnce(Transform player)
    {
        EnemyBullet bullet = _bulletPool.Get();

        bullet.transform.SetPositionAndRotation(_muzzle.position, _muzzle.rotation);
        Vector3 toPlayer = (player.position - _muzzle.position).normalized;

        if (_spreadAngle > 0f)
        {
            float halfSpread = _spreadAngle / 2f;
            float randomY = Random.Range(-halfSpread, halfSpread);
            float randomX = Random.Range(-halfSpread, halfSpread);
            Quaternion spreadRotation = Quaternion.Euler(randomX, randomY, 0f);
            toPlayer = spreadRotation * toPlayer;
        }

        bullet.transform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        bullet.Spawn(ReturnBullet, this.transform);
    }

    private void ReturnBullet(BulletBase enemyBullet)
    {
        _bulletPool.Release((EnemyBullet)enemyBullet);
    }

    private void Awake()
    {
        _bulletPool = new ObjectPool<EnemyBullet>(_bulletPrefab, this.transform, _bulletPoolSize);
    }
}
