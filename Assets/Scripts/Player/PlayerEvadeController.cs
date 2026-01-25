using UnityEngine;

public class PlayerEvadeController : MonoBehaviour
{
    public enum EvadeType
    {
        Side,
        Flip
    }

    [SerializeField] private float _evadeDuration;
    [SerializeField] private float _evadeCooldown;
    [SerializeField] private float _radius;

    [SerializeField] private float _loopHeight;
    [SerializeField] private float _sideTravelDistance;
    [SerializeField] private float _flipTravelDistance;
    [SerializeField] private float _turnSpeed;

    [SerializeField] private float _flipPitchMax;
    [SerializeField] private float _flipPitchFollwSpeed;

    private PlayerInputHandler _inputHandler;
    private PlayerAirCraftController _airCraftController;
    private PlayerHealth _health;
    private Rigidbody _rb;

    private bool _isEvading;
    private float _evadeTimer;
    private float _evadeCooldownTimer;

    private int _sideDir;
    private EvadeType _currentEvadeType;

    private Vector3 _startPos;
    private Vector3 _travel;
    private Vector3 _prevPos;
    private Quaternion _flipYawLookRotation;

    private void TryStartSideEvade()
    {
        if (_evadeCooldownTimer > 0f) return;

        int dir = _inputHandler.ConsumeSideEvadeInput();
        if (dir == 0) return;

        _sideDir = dir;
        StartEvade(EvadeType.Side);
    }

    private void TryStartFlipEvade()
    {
        if (_evadeCooldownTimer > 0f) return;
        if (!_inputHandler.ConsumeFlipEvadeInput()) return;

        StartEvade(EvadeType.Flip);
    }

    private void StartEvade(EvadeType type)
    {
        _currentEvadeType = type;
        _isEvading = true;
        _evadeTimer = 0f;
        _evadeCooldownTimer = _evadeCooldown;

        _airCraftController.DisableControl = true;
        _health.SetInvincible(true);

        _startPos = _rb.position;

        _prevPos = _rb.position;
        if (type == EvadeType.Flip)
        {
            // Flip中はYawを固定して、Pitch(X)だけ動かす
            Vector3 fwd = transform.forward;
            _flipYawLookRotation = Quaternion.LookRotation(fwd, Vector3.up);
        }

        PrepareOrbit(type);

        Debug.Log("Evade Started: " + type.ToString());
    }

    private void EndEvade()
    {
        _isEvading = false;
        _airCraftController.DisableControl = false;
        _health.SetInvincible(false);
        Debug.Log("Evade Ended");
    }

    private void PrepareOrbit(EvadeType evadeType)
    {
        switch (evadeType)
        {
            case EvadeType.Side:
                _travel = transform.right * (_sideDir * _sideTravelDistance);
                break;
            case EvadeType.Flip:
                _travel = transform.forward * _flipTravelDistance;
                break;
        }
    }

    private void UpdateLoopMovement()
    {
        float t = Mathf.Clamp01(_evadeTimer / _evadeDuration);
        float angle = Mathf.PI * 2f * t;

        Vector3 basePos = _startPos + _travel * t;

        float side = Mathf.Sin(angle) * _radius;
        float up = (1f - Mathf.Cos(angle)) * _loopHeight;

        Vector3 offset = transform.forward * side + transform.up * up;
        Vector3 newPos = basePos + offset;
        _rb.MovePosition(newPos);

        if(_currentEvadeType == EvadeType.Flip)
        {
            ApplyFlipOrientation(newPos);
        }
    }

    private void UpdateCooldownTimer()
    {
        if (_evadeCooldownTimer > 0f)
        {
            _evadeCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateEvadeTimer()
    {
        _evadeTimer += Time.deltaTime;
        if (_evadeTimer >= _evadeDuration)
        {
            EndEvade();
        }
    }

    private Vector3 GetOrbitAxis()
    {
        return _currentEvadeType switch
        {
            EvadeType.Side => transform.right,
            EvadeType.Flip => transform.forward,
            _ => transform.right
        };
    }

    private void ApplyFlipOrientation(Vector3 newPos)
    {
        Vector3 vel = (newPos - _prevPos) / Time.fixedDeltaTime;
        _prevPos = newPos;


        if (vel.sqrMagnitude < 0.0001f) return;

        // Yaw固定座標に変換して pitch を求める
        Vector3 localVel = Quaternion.Inverse(_flipYawLookRotation) * vel;

        // forward(z) と up(y) の比からピッチ角（度）を作る
        float pitch = Mathf.Atan2(localVel.y, localVel.z) * Mathf.Rad2Deg;

        // 暴れ防止
        pitch = Mathf.Clamp(pitch, -_flipPitchMax, _flipPitchMax);

        // Yaw固定 + Pitchのみ
        Quaternion targetRot = _flipYawLookRotation * Quaternion.Euler(pitch, 0f, 0f);

        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, _flipPitchFollwSpeed * Time.fixedDeltaTime));
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
        UpdateCooldownTimer();

        if (_isEvading)
        {
            UpdateEvadeTimer();
            return;
        }

        TryStartSideEvade();
        TryStartFlipEvade();
    }

    private void FixedUpdate()
    {
        if (!_isEvading) return;
        UpdateLoopMovement();
    }
}
