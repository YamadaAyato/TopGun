using System.Collections;
using UnityEngine;

/// <summary>
///     連射するタレット型の敵クラス
/// </summary>
public class TurretBurstEnemy : TurretEnemyBase
{
    [SerializeField, Tooltip("直線弾")] private EnemyBullet _bulletPrefab;
    [SerializeField, Tooltip("連射数")] private int _burstCount;
    [SerializeField, Tooltip("連射間隔")] private float _burstInterval;
    [SerializeField, Tooltip("弾のプールサイズ")] private int _bulletPoolSize;
    [SerializeField, Tooltip("弾のばらつき")] private float _spreadAngle;

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

    /// <summary>
    ///     連射を行うコルーチン
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator BurstFireCoroutine(Transform player)
    {
        // 連射回数分発射
        for (int i = 0; i < _burstCount; i++)
        {
            ShootOnce(player);
            // 連射間隔待機
            yield return new WaitForSeconds(_burstInterval);
        }
        _burstCoroutine = null;
    }

    /// <summary>
    ///     一つの弾を発射する
    /// </summary>
    /// <param name="player"></param>
    private void ShootOnce(Transform player)
    {
        EnemyBullet bullet = _bulletPool.Get();

        bullet.transform.SetPositionAndRotation(_muzzle.position, _muzzle.rotation);
        Vector3 toPlayer = (player.position - _muzzle.position).normalized;

        if (_spreadAngle > 0f)
        {
            float randomY = Random.Range(-_spreadAngle, _spreadAngle);
            float randomX = Random.Range(-_spreadAngle, _spreadAngle);
            Quaternion spreadRotation = Quaternion.Euler(randomX, randomY, 0f);
            toPlayer = spreadRotation * toPlayer;
        }

        bullet.transform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
        bullet.Spawn(ReturnBullet, this.transform);
    }

    /// <summary>
    ///     弾をプールに戻す
    /// </summary>
    /// <param name="enemyBullet"></param>
    private void ReturnBullet(BulletBase enemyBullet)
    {
        _bulletPool.Release((EnemyBullet)enemyBullet);
    }

    private void Awake()
    {
        _bulletPool = new ObjectPool<EnemyBullet>(_bulletPrefab, this.transform, _bulletPoolSize);
    }
}
