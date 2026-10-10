using UnityEngine;
using UnityEngine.Splines;

/// <summary>
///     回避動作を制御するクラス
/// </summary>
public class PlayerEvadeController : MonoBehaviour
{
    /// <summary> 回避動作中かどうかを取得する </summary>
    public bool IsEvading => _isEvading;

    [Header("参照")]
    [SerializeField] private JustEvadeDetector _justEvadeDetector;
    [SerializeField] private CounterToken _counterToken;
    [SerializeField] private CounterTargetMemory _targetMemory;
    [SerializeField] private TimeDilationController _timeDilationController;
    [SerializeField] private FlareEmitter _flareEmitter;
    [SerializeField, Tooltip("モデル")] private Transform _visual;
    [SerializeField, Tooltip("フリップ用のスプライン")] private SplineContainer _flipSpline;
    [SerializeField, Tooltip("バレルロール用のスプライン")] private SplineContainer _ballelRollSpline;
    [Header("回転動作設定")]
    [SerializeField, Tooltip("1回避での回転回数")] private float _turns;
    [SerializeField, Tooltip("回避時間")] private float _evadeDuration;
    [SerializeField, Tooltip("次の回避までのクールダウン")] private float _evadeCooldown;
    [SerializeField, Tooltip("回避中に物理影響を使うか")] private bool _useKinematicDuringEvade;
    [SerializeField] private float _justEvadeTimeDilationScale;
    [SerializeField] private float _justEvadeTimeDilationDuration;

    private PlayerInputHandler _inputHandler;
    private PlayerFlightController _airCraftController;
    private PlayerHealth _health;
    private EvasionGauge _evasionGauge;
    private Rigidbody _rb;
    private AircraftCollisionGuard _collisionGuard;
    private bool _prevKinematic;
    private bool _isEvading;
    private float _evadeTimer;
    private float _evadeCooldownTimer;
    private float _evadeForwardSpeed;
    private int _sideDir;
    private Vector3 _startPos;
    private Quaternion _startRot;
    private Quaternion _startRbRot;
    private Quaternion _visualBaseLocalRot;
    private EvadeType _currentEvadeType;

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _airCraftController = GetComponent<PlayerFlightController>();
        _health = GetComponent<PlayerHealth>();
        _evasionGauge = GetComponent<EvasionGauge>();
        _rb = GetComponent<Rigidbody>();
        _collisionGuard = GetComponent<AircraftCollisionGuard>();
    }

    private void Update()
    {
        // タイマー処理
        if (_evadeCooldownTimer > 0f)
            _evadeCooldownTimer -= Time.deltaTime;

        if (_isEvading)
        {
            if (_evadeTimer >= _evadeDuration)
                EndEvade();
            return;
        }

        if (_evadeCooldownTimer > 0f) return;

        TryStartFlipEvade();
        TryStartBarrelRollEvade();
    }

    private void FixedUpdate()
    {
        if (!_isEvading) return;

        // 回避の時間は、位置更新と同じ物理更新で進める。
        _evadeTimer = Mathf.Min(_evadeTimer + Time.fixedDeltaTime, _evadeDuration);

        UpdateEvadePosition();
        if (!enabled) return;
        _rb.MoveRotation(_startRbRot);
        UpdateVisualSpin();
    }

    /// <summary>
    ///     回避の進度を求めて移動させる
    /// </summary>
    private void UpdateEvadePosition()
    {
        float t = Mathf.Clamp01(_evadeTimer / _evadeDuration);
        Vector3 pos = EvaluateWorldPos(t);
        if (_collisionGuard != null)
        {
            if (!_collisionGuard.ConstrainMove(pos, out Vector3 safePosition)) return;

            // 軽い接触で補正した分だけ、以降の回避経路も壁から離す。
            _startPos += safePosition - pos;
            pos = safePosition;
        }
        _rb.MovePosition(pos);
    }

    /// <summary>
    ///     モデルだけを回転し演出する
    /// </summary>
    private void UpdateVisualSpin()
    {
        if (_visual == null) return;

        float t = Mathf.Clamp01(_evadeTimer / _evadeDuration);

        if (_currentEvadeType == EvadeType.Flipping)
        {
            // X軸回転（宙返り）
            float angle = 360f * _turns * t;
            _visual.localRotation = _visualBaseLocalRot * Quaternion.Euler(-angle, 0f, 0f);
        }
        else if (_currentEvadeType == EvadeType.BarrelRolling)
        {
            // Z軸回転（ロール）
            float angle = 360f * _turns * t;

            // 回避の方向に合わせて回転方向を反転
            angle *= (_sideDir == 0) ? 1 : _sideDir;
            _visual.localRotation = _visualBaseLocalRot * Quaternion.Euler(0f, 0f, -angle);
        }
    }

    /// <summary>
    ///     フリップ回避が実行できるか確認と呼び出しをする
    /// </summary>
    private void TryStartFlipEvade()
    {
        if (_flipSpline == null) return;

        if (_inputHandler.ConsumeFlipEvadeInput())
        {
            if( _evasionGauge != null && !_evasionGauge.TryConsumeCharge())
                return;

            StartEvade(EvadeType.Flipping);
            Debug.Log("Flip回避開始！");
        }
    }

    /// <summary>
    ///     バレルロール回避が実行できるか確認と呼び出しをする
    /// </summary>
    private void TryStartBarrelRollEvade()
    {
        if (_ballelRollSpline == null) return;
        int dir = _inputHandler.ConsumeSideEvadeInput();

        if (dir == 0) return;
        _sideDir = dir;

        if( _evasionGauge != null && !_evasionGauge.TryConsumeCharge())
            return;

        StartEvade(EvadeType.BarrelRolling);
        Debug.Log("Ballel Roll回避開始！");
    }

    /// <summary>
    ///     回避開始時の初期化処理等をする
    /// </summary>
    /// <param name="type"></param>
    private void StartEvade(EvadeType type)
    {
        _currentEvadeType = type;
        _isEvading = true;
        _evadeTimer = 0f;
        _evadeCooldownTimer = _evadeCooldown;

        _evasionGauge?.StartEvading();
        GameEvents.RaiseEvade();

        // 通常移動を停止する。通常回避では無敵にしない。
        _airCraftController.DisableControl = true;

        // スプラインをワールド化するための基準を保存
        _startPos = _rb.position;
        _startRot = transform.rotation;
        _startRbRot = _rb.rotation;
        _evadeForwardSpeed = _airCraftController.CurrentSpeed;
        _visualBaseLocalRot = _visual != null ? _visual.localRotation : Quaternion.identity;

        // 回避中は物理の影響受けないように
        if (_useKinematicDuringEvade)
        {
            _prevKinematic = _rb.isKinematic;
            _rb.isKinematic = true;
        }

        TryJustEvade();
    }

    /// <summary>
    ///     回避終了時の戻し処理をする
    /// </summary>
    private void EndEvade()
    {
        _isEvading = false;

        _evasionGauge?.StopEvading();

        _airCraftController.DisableControl = false;
        _health.SetInvincible(false);

        if (_useKinematicDuringEvade)
            _rb.isKinematic = _prevKinematic;

        // 回避終了後も、回避開始時の前進速度を引き継ぐ。
        if (!_rb.isKinematic)
            _rb.linearVelocity = _startRot * Vector3.forward * _evadeForwardSpeed;

        if (_visual != null)
            _visual.localRotation = _visualBaseLocalRot;

        _currentEvadeType = EvadeType.None;
        Debug.Log("Flip回避終了！");
    }

    /// <summary>
    ///     被弾していないフレームでジャスト回避を判定し、成功時の報酬を与える
    /// </summary>
    private void TryJustEvade()
    {
        if (_health.IsDead || _health.WasDamagedThisFrame) return;

        if (_justEvadeDetector.TryGetClosestBullet(transform.position, out var bullet))
        {
            // ジャスト回避が成功した場合だけ、回避終了まで無敵にする。
            _health.SetInvincible(true);
            Debug.Log("ジャスト回避成功！");
            _timeDilationController.Play(_justEvadeTimeDilationScale, _justEvadeTimeDilationDuration);
            _counterToken.AddToken(1);
            _targetMemory?.SetBullet(bullet);

            ScoreManager.Instance.AddScore(300, ScorePopupReason.JustEvade);
            AudioManager.Instance.PlaySE3D("JustEvade", transform.position);

            _evasionGauge?.RecoverCharge(1);
            GameEvents.OnJustEvade?.Invoke();

            // ホーミング弾を回避した場合の特別処理
            bool isHoming = bullet.GetComponent<EnemyHomingBullet>() != null;
            if (isHoming)
            {
                _flareEmitter?.EmitFlare();
            }
        }
    }

    /// <summary>
    ///     どのスプラインを使うかの判定をする
    ///     t地点でのスプライン上の現在位置を返す
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private Vector3 EvaluateWorldPos(float t)
    {
        Vector3 localPos;

        // EvaluatePosition(t) は、そのSpline上の t地点の位置を返す
        if (_currentEvadeType == EvadeType.Flipping)
        {
            localPos = _flipSpline.Spline.EvaluatePosition(t);

            // Splineの移動はそのままに、飛行速度による前進を加える。
            localPos.z += _evadeForwardSpeed * _evadeTimer;
        }
        else
        {
            localPos = _ballelRollSpline.Spline.EvaluatePosition(t);

            // 前進分は飛行速度から求める。
            localPos.z = _evadeForwardSpeed * _evadeTimer;

            if (_sideDir < 0)
                localPos.x *= -1f;
        }

        // 回避開始位置＋回避開始姿勢で回したローカル位置
        return _startPos + (_startRot * localPos);
    }

    private enum EvadeType
    {
        None,
        Flipping,
        BarrelRolling
    }

}
