using UnityEngine;

/// <summary>
///     単発タレット型の敵クラス
/// </summary>
public class TurretEnemy : TurretEnemyBase
{
    [Header("単発タレット設定")]
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private int _bulletPoolSize;

    private ObjectPool<EnemyBullet> _bulletPool;
    private float _timer;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        _timer = 0f;
    }

    protected override void FireAtPlayer(Transform player)
    {
        // 銃弾をプールから取得して、銃口に
        EnemyBullet enemyBullet = _bulletPool.Get();
        enemyBullet.transform.position = _muzzle.position;

        // 方向を定めて銃弾を回転させる
        Vector3 dir = (player.transform.position - _muzzle.position).normalized;
        enemyBullet.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        enemyBullet.Spawn(ReturnBullet, this.transform);
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