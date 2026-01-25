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
    [SerializeField, Tooltip("モデル")] private Transform _visual;
    [SerializeField, Tooltip("フリップ用のスプライン")] private SplineContainer _flipSpline;
    [SerializeField, Tooltip("バレルロール用のスプライン")] private SplineContainer _ballelRollSpline;

    [Header("回転動作設定")]
    [SerializeField, Tooltip("1回避での回転回数")] private float _turns;
    [SerializeField, Tooltip("回避時間")] private float _evadeDuration;
    [SerializeField, Tooltip("次の回避までのクールダウン")] private float _evadeCooldown;
    [SerializeField, Tooltip("回避中に物理影響を使うか")] private bool _useKinematicDuringEvade;

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
    private EvadeType _currnntEvadeType;

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
        if (_flipSpline == null) return;

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

    private void UpdateEvadePosition()
    {
        float t = Mathf.Clamp01(_evadeTimer / _evadeDuration);
        Vector3 pos = EvaluateWorldPos(t);
        _rb.MovePosition(pos);
    }

    private void UpdateVisualSpin()
    {
        if (_visual == null) return;

        float t = Mathf.Clamp01(_evadeTimer / _evadeDuration);

        if (_currnntEvadeType == EvadeType.Flipping)
        {
            // Flip：X軸回転（宙返り）
            float angle = 360f * _turns * t;
            _visual.localRotation = _visualBaseLocalRot * Quaternion.Euler(-angle, 0f, 0f);
        }
        else if (_currnntEvadeType == EvadeType.BallelRolling)
        {
            // Side：Z軸回転（ロール）
            float angle = 360f * _turns * t;

            // 右回避なら回転方向も合わせて反転（見た目を自然にする）
            angle *= (_sideDir == 0) ? 1 : _sideDir;

            _visual.localRotation = _visualBaseLocalRot * Quaternion.Euler(0f, 0f, -angle);
        }
    }

    private void TryStartFlipEvade()
    {
        if (_flipSpline == null) return;

        if (_inputHandler.ConsumeFlipEvadeInput())
        {
            StartEvade(EvadeType.Flipping);
            Debug.Log("Flip回避開始！");
        }
    }

    private void TryStartBallelRollEvade()
    {
        if (_ballelRollSpline == null) return;
        int dir = _inputHandler.ConsumeSideEvadeInput();

        if (dir == 0) return;
        _sideDir = dir;
        StartEvade(EvadeType.BallelRolling);
        Debug.Log("Ballel Roll回避開始！");
    }

    private void StartEvade(EvadeType type)
    {
        _currnntEvadeType = type;
        _isEvading = true;
        _evadeTimer = 0f;
        _evadeCooldownTimer = _evadeCooldown;

        _airCraftController.DisableControl = true;
        _health.SetInvincible(true);

        _startPos = _rb.position;
        _startRot = transform.rotation;

        _startRbRot = _rb.rotation;

        _visualBaseLocalRot = _visual != null ? _visual.localRotation : Quaternion.identity;

        if (_useKinematicDuringEvade)
        {
            _prevKinematic = _rb.isKinematic;
            _rb.isKinematic = true;
        }
    }

    private void EndEvade()
    {
        _isEvading = false;

        _airCraftController.DisableControl = false;
        _health.SetInvincible(false);

        if (_useKinematicDuringEvade)
            _rb.isKinematic = _prevKinematic;

        if (_visual != null)
            _visual.localRotation = _visualBaseLocalRot;

        _currnntEvadeType = EvadeType.None;
        Debug.Log("Flip回避終了！");
    }

    private Vector3 EvaluateWorldPos(float t)
    {
        Vector3 localPos;

        if (_currnntEvadeType == EvadeType.Flipping)
        {
            localPos = _flipSpline.Spline.EvaluatePosition(t);
        }
        else // Side
        {
            localPos = _ballelRollSpline.Spline.EvaluatePosition(t);

            if (_sideDir < 0)
                localPos.x *= -1f;
        }

        // 回避開始位置＋回避開始姿勢で回したローカル位置
        return _startPos + (_startRot * localPos);
    }
}
