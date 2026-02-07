using UnityEngine;

/// <summary>
///     ホーミング弾を撃つタレット型敵クラス
/// </summary>
public class TurretHomingEnemy : TurretEnemyBase
{
    [Header("ホーミングタレット設定")]
    [SerializeField] private EnemyHomingBullet _bullet;
    [SerializeField, Tooltip("弾のプールサイズ")] private int _bulletPoolSize;
    [SerializeField, Tooltip("タレット自体の回転速度")] private float _rotationSpeed;

    [Header("ロックオン設定")]
    [SerializeField, Tooltip("撃つのに必要なロックオン時間")] private float _lockOnTime;
    [SerializeField, ReadOnly] private float _lockOnTimer;

    private bool _canStartLock;
    private bool _isLocking;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        _isLocking = false;
        _lockOnTimer = 0f;
    }

    protected override void FireAtPlayer(Transform player)
    {
        _shooter.Fire(this.transform, player);
    }

    /// <summary>
    ///     タレットをターゲットに向けてy軸回転させる
    /// </summary>
    /// <param name="target"></param>
    private void RotateTurret(Transform target)
    {
        // ターゲットへの方向ベクトルを計算
        Vector3 dir = target.position - this.transform.position;
        // y軸回転のみとするため、y成分を0にする
        dir.y = 0;

        // 目標の回転を計算
        Quaternion desired = Quaternion.LookRotation(dir.normalized, Vector3.up);
        // 現在の回転から目標の回転へ徐々に回転させる
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desired,
            _rotationSpeed * Time.deltaTime
        );
    }

    protected override void Update()
    {
        _timer += Time.deltaTime;

        // 発射可能かどうかの判定
        _canStartLock = _timer >= _fireInterval;
        bool hasPlayer =
            TryGetPlayer(out Transform player) &&
            IsPlayerInRange(player) &&
            IsPlayerInFov(player) &&
            HasLineOfSight(player);

        // ロックオン処理
        if (hasPlayer && _canStartLock)
        {
            // ロックオン開始
            if (!_isLocking)
            {
                _isLocking = true;
                _lockOnTimer = 0f;
            }

            _lockOnTimer += Time.deltaTime;
            RotateTurret(player);

            // ロックオン完了で発射
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
            // ロックオン解除
            _isLocking = false;
            _lockOnTimer = 0f;
        }
    }
}
