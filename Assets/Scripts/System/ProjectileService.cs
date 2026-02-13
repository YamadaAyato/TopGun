using UnityEngine;

/// <summary>
///     弾のプールを共通管理するクラス
/// </summary>
public class ProjectileService : MonoBehaviour
{
    public static ProjectileService Instance { get; private set; }

    [Header("参照")]
    [SerializeField] private EnemyStraightBullet _straightBulletPrefab;
    [SerializeField] private EnemyHomingBullet _homingBulletPrefab;
    [SerializeField] private ExplosionFx _smallExplosionEffectPrefab;
    [SerializeField] private ExplosionFx _bigExplosionEffectPrefab;
    [SerializeField, Tooltip("直線弾の親")] private Transform _straightParent;
    [SerializeField, Tooltip("ホーミング弾の親")] private Transform _homingParent;
    [SerializeField, Tooltip("小さい爆発エフェクトの親")] private Transform _smallExplosionEffectParent;  
    [SerializeField, Tooltip("大きい爆発エフェクトの親")] private Transform _bigExplosionEffectParent;

    [Header("プールサイズ設定")]
    [SerializeField, Tooltip("直線弾のプールサイズ")] private int _straightBulletPoolSize;
    [SerializeField, Tooltip("ホーミング弾のプールサイズ")] private int _homingBulletPoolSize;
    [SerializeField, Tooltip("爆発エフェクトのプールサイズ")] private int _smallExplosionPoolSize;
    [SerializeField, Tooltip("爆発エフェクトのプールサイズ")] private int _bigeExplosionPoolSize;

    private ObjectPool<EnemyStraightBullet> _straightPool;
    private ObjectPool<EnemyHomingBullet> _homingPool;
    private ObjectPool<ExplosionFx> _smallExplosionPool;
    private ObjectPool<ExplosionFx> _bigExplosionPool;

    /// <summary>
    ///     直線弾をスポーンする
    /// </summary>
    /// <param name="muzzle"> 銃口 </param>
    /// <param name="shooter"> 撃ち手 </param>
    /// <param name="dir"> 方向 </param>
    /// <returns></returns>
    public EnemyStraightBullet SpawnStraightBullet(Transform muzzle, Transform shooter, Vector3 dir)
    {
        EnemyStraightBullet bullet = _straightPool.Get();
        bullet.transform.position = muzzle.position;
        bullet.Spawn(ReturnStraightBullet, shooter);
        bullet.SetDirection(dir);
        return bullet;
    }

    /// <summary>
    ///     ホーミング弾をスポーンする
    /// </summary>
    /// <param name="muzzle"> 銃口 </param>
    /// <param name="shooter"> 撃ち手 </param>
    /// <param name="dir"> ターゲット </param>
    /// <returns></returns>
    public EnemyHomingBullet SpawnHomingBullet(Transform muzzle, Transform shooter, Transform target)
    {
        EnemyHomingBullet bullet = _homingPool.Get();
        bullet.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
        bullet.Spawn(ReturnHomingBullet, shooter); ;
        bullet.SetTarget(target, muzzle.forward);
        return bullet;
    }

    /// <summary>
    ///     爆発エフェクトをスポーンする
    /// </summary>
    /// <param name="point"> 銃口 </param>
    /// <param name="shooter"> 撃ち手 </param>
    /// <param name="dir"> ターゲット </param>
    /// <returns></returns>
    public ExplosionFx SpawnExplosion(ExplosionType type,Transform point)
    {
        ExplosionFx explosion = type switch
        {
            ExplosionType.Small => _smallExplosionPool.Get(),
            ExplosionType.Big => _bigExplosionPool.Get(),
            _ => null
        };

        explosion.transform.SetPositionAndRotation(point.position, point.rotation);
        explosion.Play(type switch
        {
            ExplosionType.Small => ReturnSmallExplosion,
            ExplosionType.Big => ReturnBigExplosion,
            _ => null
        });

        return explosion;
    }

    /// <summary>
    ///     直線弾をプールに返す
    /// </summary>
    /// <param name="bullet"></param>
    private void ReturnStraightBullet(BulletBase bullet)
    {
        _straightPool.Release((EnemyStraightBullet)bullet);
    }

    private void ReturnHomingBullet(BulletBase bullet)
    {
        _homingPool.Release((EnemyHomingBullet)bullet);
    }

    private void ReturnSmallExplosion(ExplosionFx explosion)
    {
        _smallExplosionPool.Release(explosion);
    }

    private void ReturnBigExplosion(ExplosionFx explosion)
    {
        _bigExplosionPool.Release(explosion);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        _straightPool = new ObjectPool<EnemyStraightBullet>(
            _straightBulletPrefab, _straightParent, _straightBulletPoolSize);
        _homingPool = new ObjectPool<EnemyHomingBullet>(
            _homingBulletPrefab, _homingParent, _homingBulletPoolSize);
        _smallExplosionPool = new ObjectPool<ExplosionFx>(
            _smallExplosionEffectPrefab, _smallExplosionEffectParent, _smallExplosionPoolSize);
        _bigExplosionPool = new ObjectPool<ExplosionFx>(
            _bigExplosionEffectPrefab, _bigExplosionEffectParent, _bigeExplosionPoolSize);
    }
}
