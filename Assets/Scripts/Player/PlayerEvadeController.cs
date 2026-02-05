using UnityEngine;
using UnityEngine.Splines;

/// <summary>
///     回避動作を制御するクラス
/// </summary>
public class PlayerEvadeController : MonoBehaviour
{
    private enum EvadeType
    {
        None,
        Flipping,
        BallelRolling
    }

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
    private PlayerAirCraftController _airCraftController;
    private PlayerHealth _health;
    private Rigidbody _rb;

    private bool _prevKinematic;
    private bool _isEvading;
    private float _evadeTimer;
    private float _evadeCooldownTimer;
    private int _sideDir;

    private Vector3 _startPos;
    private Quaternion _startRot;
    private Quaternion _startRbRot;
    private Quaternion _visualBaseLocalRot;
    private EvadeType _currntEvadeType;

    /// <summary>
    ///     回避の進度を求めて移動させる
    /// </summary>
    private void UpdateEvadePosition()
    {
        float t = Mathf.Clamp01(_evadeTimer / _evadeDuration);
        Vector3 pos = EvaluateWorldPos(t);
        _rb.MovePosition(pos);
    }

    /// <summary>
    ///     モデルだけを回転し演出する
    /// </summary>
    private void UpdateVisualSpin()
    {
        if (_visual == null) return;

        float t = Mathf.Clamp01(_evadeTimer / _evadeDuration);

        if (_currntEvadeType == EvadeType.Flipping)
        {
            // X軸回転（宙返り）
            float angle = 360f * _turns * t;
            _visual.localRotation = _visualBaseLocalRot * Quaternion.Euler(-angle, 0f, 0f);
        }
        else if (_currntEvadeType == EvadeType.BallelRolling)
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
            StartEvade(EvadeType.Flipping);
            Debug.Log("Flip回避開始！");
        }
    }

    /// <summary>
    ///     バレルロール回避が実行できるか確認と呼び出しをする
    /// </summary>
    private void TryStartBallelRollEvade()
    {
        if (_ballelRollSpline == null) return;
        int dir = _inputHandler.ConsumeSideEvadeInput();

        if (dir == 0) return;
        _sideDir = dir;
        StartEvade(EvadeType.BallelRolling);
        Debug.Log("Ballel Roll回避開始！");
    }

    /// <summary>
    ///     回避開始時の初期化処理等をする
    /// </summary>
    /// <param name="type"></param>
    private void StartEvade(EvadeType type)
    {
        _currntEvadeType = type;
        _isEvading = true;
        _evadeTimer = 0f;
        _evadeCooldownTimer = _evadeCooldown;

        // 通常移動を停止、無敵ON
        _airCraftController.DisableControl = true;
        _health.SetInvincible(true);

        // スプラインをワールド化するための基準を保存
        _startPos = _rb.position;
        _startRot = transform.rotation;
        _startRbRot = _rb.rotation;
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

        _airCraftController.DisableControl = false;
        _health.SetInvincible(false);

        if (_useKinematicDuringEvade)
            _rb.isKinematic = _prevKinematic;

        if (_visual != null)
            _visual.localRotation = _visualBaseLocalRot;

        _currntEvadeType = EvadeType.None;
        Debug.Log("Flip回避終了！");
    }

    private void TryJustEvade()
    {
        if (_justEvadeDetector.TryGetClosestBullet(transform.position, out var bullet))
        {
            Debug.Log("ジャスト回避成功！");
            _timeDilationController.Play(_justEvadeTimeDilationScale, _justEvadeTimeDilationDuration);
            _counterToken.AddToken(1);
            _targetMemory?.SetBullet(bullet);

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
        if (_currntEvadeType == EvadeType.Flipping)
        {
            localPos = _flipSpline.Spline.EvaluatePosition(t);
        }
        else
        {
            localPos = _ballelRollSpline.Spline.EvaluatePosition(t);

            if (_sideDir < 0)
                localPos.x *= -1f;
        }

        // 回避開始位置＋回避開始姿勢で回したローカル位置
        return _startPos + (_startRot * localPos);
    }

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _airCraftController = GetComponent<PlayerAirCraftController>();
        _health = GetComponent<PlayerHealth>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // タイマー処理
        if (_evadeCooldownTimer > 0f)
            _evadeCooldownTimer -= Time.deltaTime;

        if (_isEvading)
        {
            _evadeTimer += Time.deltaTime;
            if (_evadeTimer >= _evadeDuration)
                EndEvade();
            return;
        }

        if (_evadeCooldownTimer > 0f) return;

        TryStartFlipEvade();
        TryStartBallelRollEvade();
    }

    private void FixedUpdate()
    {
        if (!_isEvading) return;

        UpdateEvadePosition();
        _rb.MoveRotation(_startRbRot);
        UpdateVisualSpin();
    }
}
