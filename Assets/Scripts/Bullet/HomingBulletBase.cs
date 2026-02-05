using UnityEngine;

/// <summary>
///     ホーミング弾の基底クラス
/// </summary>
public abstract class HomingBulletBase : BulletBase, IDecoyAttractable
{
    [Header("ホーミング設定")]
    [SerializeField, Tooltip("追尾旋回速度")] private float _turnSpeed;
    [SerializeField, Tooltip("ホーミングする最大時間")] private float _homingDuration;
    [SerializeField, Tooltip("追尾できる角度制限")] private float _maxSeekAngle;
    [SerializeField, Tooltip("前方へ発射する時間")] private float _initialForwardTime;

    private Transform _defaultTarget;
    private Transform _decoyTarget;
    private float _homingTimer;
    private float _forwardTimer;
    private Rigidbody _rb;
    private Quaternion _launchRotation;
    private Vector3 _launchForward;

    /// <summary>
    ///     ターゲットを設定する
    /// </summary>
    /// <param name="target"> 狙う敵 </param>
    /// <param name="shooterForward"> 撃ち手の前方 </param>
    public void SetTarget(Transform target, Vector3 shooterForward)
    {
        _defaultTarget = target;
        _launchForward = shooterForward.sqrMagnitude > 0.0001f ? shooterForward.normalized : transform.forward;
    }

    public void SetDecoyTarget(Transform decoyTransform)
    {
        _decoyTarget = decoyTransform;
    }

    public void ClearDecoyTarget(Transform decoyTransform)
    {
        if (_decoyTarget == decoyTransform)
            _decoyTarget = null;
    }

    private Transform _currentTarget => _decoyTarget != null ? _decoyTarget : _defaultTarget;

    protected override void OnSpawned()
    {
        _homingTimer = 0f;
        _forwardTimer = 0f;

        if (_rb == null) _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        _launchRotation = _rb.rotation;
        _launchForward = transform.forward;
    }

    private void FixedUpdate()
    {
        // 寿命管理
        _timer += Time.fixedDeltaTime;
        if (_timer >= _lifeTime)
        {
            Release();
            return;
        }

        Transform target = _currentTarget;

        // ホーミング処理
        bool canHome = target != null;
        if (_homingDuration > 0f)
        {
            _homingTimer += Time.fixedDeltaTime;
            if (_homingTimer > _homingDuration) canHome = false;
        }

        // 前進フェーズ管理
        _forwardTimer += Time.fixedDeltaTime;
        bool isForwardPhase = (_forwardTimer < _initialForwardTime);

        Quaternion nextRot = _rb.rotation;

        if (isForwardPhase)
        {
            // 発射直後は撃ち手の向きで直進
            nextRot = _launchRotation;
        }
        else if (canHome)
        {
            Vector3 toTarget = target.position - _rb.position;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;

                // 目標方向と現在の前方ベクトルの角度を計算
                float angle = Vector3.Angle(transform.forward, dir);
                // 角度が制限内ならば回転を行う
                if (angle <= _maxSeekAngle)
                {
                    // 目標方向への回転を計算
                    Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
                    float maxStep = _turnSpeed * Time.fixedDeltaTime;
                    nextRot = Quaternion.RotateTowards(_rb.rotation, desired, maxStep);
                }
            }
        }

        _rb.MoveRotation(nextRot);
        float speedNow = _bulletSpeed;

        // フェーズによって移動方向を決定
        Vector3 moveDir = isForwardPhase ? _launchForward : (nextRot * Vector3.forward);
        Vector3 nextPos = _rb.position + moveDir * speedNow * Time.fixedDeltaTime;
        _rb.MovePosition(nextPos);
    }

    protected override void Update() { }
}
