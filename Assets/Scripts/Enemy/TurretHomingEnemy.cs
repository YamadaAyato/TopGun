using UnityEngine;

public class TurretHomingEnemy : TurretEnemyBase
{
    [Header("ホーミングタレット設定")]
    [SerializeField] private EnemyHomingBullet _bullet;
    [SerializeField] private int _bulletPoolSize;

    [SerializeField] private float _lockOnTime;
    [SerializeField] private float _rotationSpeed;

    private ObjectPool<EnemyHomingBullet> _bulletPool;
    private bool _canStartLock;
    private bool _isLocking;
    private float _lockOnTimer;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        _isLocking = false;
        _lockOnTimer = 0f;
    }

    protected override void FireAtPlayer(Transform player)
    {
        EnemyHomingBullet bullet = _bulletPool.Get();

        bullet.transform.SetPositionAndRotation(_muzzle.position, _muzzle.rotation);
        bullet.Spawn(ReturnBullet, this.transform);
        bullet.SetTarget(player, _muzzle.forward);
    }

    private void Rotate(Transform target)
    {
        Vector3 dir = target.position - this.transform.position;
        dir.y = 0;

        Quaternion desired = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desired,
            _rotationSpeed * Time.deltaTime
        );
    }

    private void ReturnBullet(BulletBase bullet)
    {
        _bulletPool.Release((EnemyHomingBullet)bullet);
    }

    protected override void Update()
    {
        _timer += Time.deltaTime;
        _canStartLock = _timer >= _fireInterval;

        bool hasPlayer =
            TryGetPlayer(out Transform player) &&
            IsPlayerInRange(player) &&
            IsPlayerInFov(player) &&
            HasLineOfSight(player);

        if (hasPlayer && _canStartLock)
        {
            if (!_isLocking)
            {
                _isLocking = true;
                _lockOnTimer = 0f;
            }

            _lockOnTimer += Time.deltaTime;
            Rotate(player);

            if (_lockOnTimer >= _lockOnTime)
            {
                FireAtPlayer(player);
                _timer = 0f;
                _isLocking = false;
                _lockOnTimer = 0f;
            }
        }
        else
        {
            _isLocking = false;
            _lockOnTimer = 0f;
        }
    }

    private void Awake()
    {
        _bulletPool = new ObjectPool<EnemyHomingBullet>(_bullet, this.transform, _bulletPoolSize);
    }
}
