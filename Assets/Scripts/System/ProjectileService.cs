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
    [SerializeField] private Transform _straightParent;
    [SerializeField] private Transform _homingParent;

    [Header("プールサイズ設定")]
    [SerializeField] private int _straightBulletPoolSize;
    [SerializeField] private int _homingBulletPoolSize;

    private ObjectPool<EnemyStraightBullet> _straightPool;
    private ObjectPool<EnemyHomingBullet> _homingPool;

    public EnemyStraightBullet SpawnStraightBullet(Transform muzzle, Transform shooter, Vector3 dir)
    {
        EnemyStraightBullet bullet = _straightPool.Get();
        bullet.transform.position = muzzle.position;
        bullet.Spawn(ReturnStraightBullet, shooter);
        bullet.SetDirection(dir);
        return bullet;
    }

    public EnemyHomingBullet SpawnHomingBullet(Transform muzzle, Transform shooter, Transform target)
    {
        EnemyHomingBullet bullet = _homingPool.Get();
        bullet.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
        bullet.Spawn(ReturnHomingBullet, shooter); ;
        bullet.SetTarget(target,muzzle.forward);
        return bullet;
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
    }
}
