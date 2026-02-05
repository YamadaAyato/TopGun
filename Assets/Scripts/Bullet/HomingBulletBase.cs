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

    [Header("デコイ吸い込み")]
    [SerializeField, Tooltip("デコイ中は移動方向もターゲットへ向けるか")]
    private bool _decoySnapMoveToTarget = true;

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

        // ホーミング可能か
        bool canHome = target != null;
        if (_homingDuration > 0f)
        {
            _homingTimer += Time.fixedDeltaTime;
            if (_homingTimer > _homingDuration) canHome = false;
        }

        // 前進フェーズ管理
        _forwardTimer += Time.fixedDeltaTime;
        bool isForwardPhase = (_forwardTimer < _initialForwardTime);

        // 初期化
        Quaternion nextRot = _rb.rotation;
        Vector3 dir = Vector3.zero;
        bool hasDir = false;

        if (isForwardPhase)
        {
            // 発射直後は撃ち手の向きで直進
            nextRot = _launchRotation;
        }
        else if (canHome && !isForwardPhase)
        {
            Vector3 toTarget = target.position - _rb.position;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                dir = toTarget.normalized;
                hasDir = true;

                // 目標方向への回転を計算
                Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);

                if (_decoyTarget != null)
                {
                    // デコイ吸い込み中は即座に目標方向へ向ける
                    nextRot = desired;
                }
                else
                {
                    // 目標方向と現在の前方ベクトルの角度を計算
                    Vector3 currentForward = _rb.rotation * Vector3.forward;
                    float angle = Vector3.Angle(currentForward, dir);

                    // 0〜maxSeekAngleを 1〜0 に落とす（範囲外は0）
                    float t = Mathf.InverseLerp(_maxSeekAngle, 0f, angle);
                    float weight = Mathf.Clamp01(t);

                    // 完全に0にしたくない最低値を持たせる
                    // 調整用
                    float minWeight = 0.15f;
                    weight = Mathf.Lerp(minWeight, 1f, weight);

                    float maxStep = _turnSpeed * weight * Time.fixedDeltaTime;
                    nextRot = Quaternion.RotateTowards(_rb.rotation, desired, maxStep);
                }
            }
        }

        _rb.MoveRotation(nextRot);

        Vector3 moveDir;
        // フェーズによって移動方向を決定
        if (isForwardPhase)
        {
            moveDir = _launchForward;
        }
        else if (_decoyTarget != null && _decoySnapMoveToTarget && hasDir)
        {
            moveDir = dir;
        }
        else
        {
            moveDir = nextRot * Vector3.forward;
        }

        Vector3 nextPos = _rb.position + moveDir * _bulletSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(nextPos);
    }

    protected override void Update() { }
}
