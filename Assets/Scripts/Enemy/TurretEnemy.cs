using UnityEngine;

/// <summary>
///     タレット型の敵クラス
/// </summary>
public class TurretEnemy : EnemyBase
{
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private float _fireInterval;
    [SerializeField] private int _bulletPoolSize;

    // デバッグ用で仮のシリアライズ
    [SerializeField] private PlayerHealth _targetPlayer;

    private ObjectPool<EnemyBullet> _bulletPool;
    private float _timer;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        _timer = 0f;
    }

    /// <summary>
    ///     銃弾をプールから取得して発射する
    /// </summary>
    private void Fire()
    {
        // 銃弾をプールから取得して、銃口に
        EnemyBullet enemyBullet = _bulletPool.Get();
        enemyBullet.transform.position = _muzzle.position;

        // 方向を定めて銃弾を回転させる
        Vector3 dir = (_targetPlayer.transform.position - this.transform.position).normalized;
        enemyBullet.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        enemyBullet.Spawn(ReturnBullet);
    }

    private void ReturnBullet(BulletBase enemyBullet)
    {
        _bulletPool.Release((EnemyBullet)enemyBullet);
    }

    private void Awake()
    {
        _bulletPool = new ObjectPool<EnemyBullet>(_bulletPrefab, this.transform, _bulletPoolSize);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > _fireInterval)
        {
            _timer = 0f;
            Fire();
        }
    }
}