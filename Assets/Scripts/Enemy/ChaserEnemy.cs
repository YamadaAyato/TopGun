using UnityEngine;

public class ChaserEnemy : EnemyBase
{
    [Header("現在の移動速度")]
    [SerializeField, ReadOnly] private float _currentSpeed;

    [Header("参照")]
    [SerializeField] private EnemyShooter _shooter;

    [Header("回転設定")]
    [SerializeField, Tooltip("1秒あたりの最大旋回角度")] private float _turnSpeedDeg;

    [Header("速度制御")]
    [SerializeField, Tooltip("最低移動速度")] private float _minSpeed;
    [SerializeField, Tooltip("最高移動速度")] private float _maxSpeed ;
    [SerializeField, Tooltip("この距離以上で最高速度に近づく")] private float _speedMaxDistance;
    [SerializeField, Tooltip("この距離以下で最低速度に近づく")] private float _speedMinDistance;
    [SerializeField, Tooltip("旋回中にどれだけ減速するか（0=減速なし）")] private float _turnSlowdown;
    [SerializeField, Tooltip("速度変化のなめらかさ")] private float _speedResponse;

    [Header("回り込み挙動")]
    [SerializeField, Tooltip("この距離以内で回り込みを強める")] private float _orbitDistance;
    [SerializeField, Tooltip("横方向への最大オフセット量")] private float _orbitRadius;
    [SerializeField, Tooltip("上方向へのオフセット量（下から来ないため）")] private float _liftAmount    ;
    [SerializeField, Tooltip("回り込み方向を更新する間隔")] private float _rerollInterval;
    [SerializeField, Tooltip("プレイヤーに近づきすぎないための最小距離")] private float _minSeparation;

    [Header("障害物回避")]
    [SerializeField, Tooltip("前方に障害物を検知する距離")] private float _avoidDistance;
    [SerializeField, Tooltip("回避オフセットの強さ")] private float _avoidStrength;
    [SerializeField, Tooltip("障害物として扱うレイヤー")] private LayerMask _obstacleLayer;

    [Header("ロール演出（見た目）")]
    [SerializeField, Tooltip("ロール演出を適用する見た目用Transform")] private Transform _visual;
    [SerializeField, Tooltip("最大ロール角度")] private float _rollMaxAngle;
    [SerializeField, Tooltip("ロールの追従速度")] private float _rollResponse;

    [Header("攻撃")]
    [SerializeField, Tooltip("射撃可能な最大距離")] private float _attackRange;
    [SerializeField, Tooltip("射撃可能な視野角")] private float _fovAngle;
    [SerializeField, Tooltip("射撃クールダウン")] private float _fireCooldown;

    // 回り込み方向（プレイヤー基準のローカル方向）
    private Vector3 _orbitDirLocal = new Vector3(1f, 0.3f, 0f);
    private float _rerollTimer;
    private float _cooldownTimer;
    // 見た目用の基準回転
    private Quaternion _visualBaseLocalRot = Quaternion.identity;

    /// <summary>
    ///     タイマー更新をする
    ///     今は射撃クールダウンの更新のみをする
    /// </summary>
    private void UpdateTimers()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    /// <summary>
    ///     一定時間ごとに回り込み方向を変える
    /// </summary>
    private void UpdateOrbitDirectionTimer()
    {
        _rerollTimer -= Time.deltaTime;
        if (_rerollTimer <= 0f)
        {
            _rerollTimer = _rerollInterval;
            RerollOrbitDirection();
        }
    }

    /// <summary>
    ///     プレイヤー周囲の回り込みを考慮した狙い地点を計算する
    /// </summary>
    private Vector3 ComputeAimPoint(Transform player)
    {
        float dist = Vector3.Distance(transform.position, player.position);
        // 近いほど回り込みを強くする係数を作る
        float near01 = Mathf.Clamp01(1f - dist / _orbitDistance);

        // プレイヤーのローカル空間での方向をワールド基準に
        Vector3 orbitWorldDir = player.TransformDirection(_orbitDirLocal).normalized;

        // オフセットを作って狙う地点を決める
        Vector3 offset =
            orbitWorldDir * (_orbitRadius * near01) +
            Vector3.up * (_liftAmount * near01);

        return player.position + offset;
    }

    /// <summary>
    ///     前方に障害物がある場合、狙い地点をずらして回避する
    /// </summary>
    private void ApplyObstacleAvoidance(ref Vector3 aimPoint)
    {
        // 前方に障害物があるか確認する
        if (!Physics.Raycast(transform.position, transform.forward,
            out RaycastHit hit, _avoidDistance, _obstacleLayer))
            return;

        // 壁から離れる方向（水平）を作る
        Vector3 away =
            Vector3.ProjectOnPlane(hit.normal, Vector3.up).normalized;

        if (away.sqrMagnitude < 0.001f)
            away = transform.right;

        // 近いほど強く避ける係数を作る
        float t = 1f - hit.distance / _avoidDistance;
        aimPoint += away * _avoidStrength * t * _orbitRadius;
    }

    /// <summary>
    ///     狙い地点へ向かって旋回する
    /// </summary>
    private void RotateTowardsAimPoint(Vector3 aimPoint)
    {
        // 方向を計算
        Vector3 toAim = aimPoint - transform.position;
        if (toAim.sqrMagnitude < 0.0001f) return;

        // 旋回角速度に制限を付けて回転
        Quaternion desired = Quaternion.LookRotation(toAim.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, desired, _turnSpeedDeg * Time.deltaTime);
    }

    /// <summary>
    ///     距離と旋回量から目標速度を算出し、現在速度を更新する
    /// </summary>
    private void UpdateMovementSpeed(Transform player, Vector3 aimPoint)
    {
        // 距離から基本速度を作る
        // 近い→遅い / 遠い→速い を作る
        float dist = Vector3.Distance(transform.position, player.position);
        float t = Mathf.InverseLerp(_speedMinDistance, _speedMaxDistance, dist);
        float speedByDistance = Mathf.Lerp(_minSpeed, _maxSpeed, t);

        // 旋回量で減速する
        Vector3 toAim = (aimPoint - transform.position).normalized;
        float dot = Mathf.Clamp(Vector3.Dot(transform.forward, toAim), -1f, 1f);
        float turn01 = 1f - Mathf.InverseLerp(0.7f, 1f, dot);

        float targetSpeed = speedByDistance * (1f - turn01 * _turnSlowdown);
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _speedResponse * Time.deltaTime);
    }

    /// <summary>
    ///     前方へ移動する
    /// </summary>
    private void MoveForward()
    {
        transform.position += transform.forward * _currentSpeed * Time.deltaTime;
    }

    /// <summary>
    ///     プレイヤーに近づきすぎた場合に押し戻す
    /// </summary>
    private void ResolveTooClose(Transform player)
    {
        Vector3 fromPlayer = transform.position - player.position;
        float dist = fromPlayer.magnitude;
        if (dist < _minSeparation && dist > 0.0001f)
        {
            transform.position += fromPlayer.normalized * (_minSeparation - dist);
        }
    }

    /// <summary>
    ///     見た目用のロール演出を更新する
    /// </summary>
    private void UpdateRollVisual(Vector3 aimPoint)
    {
        if (_visual == null) return;

        // どっち側に曲がろうとしてるか測る
        Vector3 toAim = (aimPoint - transform.position).normalized;
        float side = Vector3.Dot(transform.right, toAim);

        // 右に傾くっぽい見た目にしたいので符号を反転する
        float targetRoll = -side * _rollMaxAngle;
        float currentRoll = NormalizeAngle(_visual.localEulerAngles.z);
        float nextRoll = Mathf.Lerp(currentRoll, targetRoll, _rollResponse * Time.deltaTime);

        // 基準回転にロールを足して適用する
        _visual.localRotation =
            _visualBaseLocalRot * Quaternion.Euler(0f, 0f, nextRoll);
    }

    /// <summary>
    ///     射撃条件を満たしていれば発射する
    /// </summary>
    private void TryShoot(Transform player)
    {
        // 距離チェック
        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > _attackRange) return;

        // 視野角チェック
        Vector3 dir = toPlayer.normalized;
        float cos = Mathf.Cos(_fovAngle * 0.5f * Mathf.Deg2Rad);
        if (Vector3.Dot(transform.forward, dir) < cos) return;

        if (_cooldownTimer > 0f) return;

        // 撃つ
        _cooldownTimer = _fireCooldown;
        _shooter?.Fire(transform, player);
    }

    /// <summary>
    /// 回り込み方向をランダムに再生成する。
    /// </summary>
    private void RerollOrbitDirection()
    {
        Vector3 v = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.2f, 0.9f),
            Random.Range(-1f, 1f));

        if (v.sqrMagnitude < 0.0001f)
            v = new Vector3(1f, 0.3f, 0f);

        _orbitDirLocal = v.normalized;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private void Start()
    {
        if (_visual != null)
            _visualBaseLocalRot = _visual.localRotation;

        RerollOrbitDirection();
        _rerollTimer = _rerollInterval;

        _currentSpeed = _minSpeed;
    }

    private void Update()
    {
        // プレイヤー取得
        Transform player = PlayerLocator.Instance?.PlayerTransform;
        if (player == null) return;

        UpdateTimers();
        UpdateOrbitDirectionTimer();

        Vector3 aimPoint = ComputeAimPoint(player);
        ApplyObstacleAvoidance(ref aimPoint);

        RotateTowardsAimPoint(aimPoint);
        UpdateMovementSpeed(player, aimPoint);
        MoveForward();
        ResolveTooClose(player);

        UpdateRollVisual(aimPoint);

        TryShoot(player);
    }
}
