using UnityEngine;
using UnityEngine.Splines;

/// <summary>
///     回避動作を制御するクラス
/// </summary>
public class PlayerEvadeController : MonoBehaviour
{
    [SerializeField] private Transform _visual;
    [SerializeField] private float _flipTurns;
    [SerializeField] private SplineContainer _flipSpline;
    [SerializeField] private float _evadeDuration;
    [SerializeField] private float _evadeCooldown;
    [SerializeField] private bool _useKinematicDuringEvade;

    private PlayerInputHandler _inputHandler;
    private PlayerAirCraftController _airCraftController;
    private PlayerHealth _health;
    private Rigidbody _rb;

    private bool _isEvading;
    private float _evadeTimer;
    private float _evadeCooldownTimer;

    private Vector3 _startPos;
    private Quaternion _startRot;
    private Quaternion _startRbRot;
    private Quaternion _visualBaseLocalRot;

    private bool _prevKinematic;

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _airCraftController = GetComponent<PlayerAirCraftController>();
        _health = GetComponent<PlayerHealth>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
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

        if (_inputHandler.ConsumeFlipEvadeInput())
        {
            StartFlipEvade();
        }
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

        float spinAngle = 360f * _flipTurns * t;

        _visual.localRotation = _visualBaseLocalRot * Quaternion.Euler(-spinAngle, 0f, 0f);
    }

    private void StartFlipEvade()
    {
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

        Debug.Log("Flip回避開始！！");
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

        Debug.Log("Flip回避終了！");
    }

    private Vector3 EvaluateWorldPos(float t)
    {
        Vector3 localPos = _flipSpline.Spline.EvaluatePosition(t);
        return _startPos + (_startRot * localPos);
    }
}
