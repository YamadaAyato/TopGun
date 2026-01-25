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

    [SerializeField] private float _pivotDistace;
    [SerializeField] private float _pivotHeightOffsetCommon;
    [SerializeField] private float _pivotHeightOffsetSideEvade;
    [SerializeField] private float _pivotHeightOffsetFlipEvade;

    private PlayerInputHandler _inputHandler;
    private PlayerAirCraftController _airCraftController;
    private PlayerHealth _health;
    private Rigidbody _rb;

    private bool _isEvading;
    private float _evadeTimer;
    private float _evadeCooldownTimer;

    private int _sideDir;

    private Vector3 _pivot;
    private Vector3 _startOffset;
    private EvadeType _currentEvadeType;

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
                PrepareSideorbit();
                break;
            case EvadeType.Flip:
                PrepareFlipOrbit();
                break;
        }

        _rb.MovePosition(_pivot + _startOffset);
    }

    private void PrepareSideorbit()
    {
        Vector3 side = transform.right * _sideDir;

        _pivot = _rb.position + side * _pivotHeightOffsetSideEvade
            + Vector3.up * _pivotHeightOffsetCommon;

        Vector3 offset = _rb.position - _pivot;
        _startOffset = offset.normalized * _radius;
    }

    private void PrepareFlipOrbit()
    {
        _pivot = _rb.position + Vector3.up * _pivotHeightOffsetFlipEvade;
        Vector3 offset = _rb.position - _pivot;
        _startOffset = offset.normalized * _radius;
    }

    private void UpdateOrbitMovement()
    {
        float t = Mathf.Clamp01(_evadeTimer / _evadeDuration);
        float angle = 360f * t;

        Vector3 axis = GetOrbitAxis();
        Quaternion rot = Quaternion.AngleAxis(angle, axis);

        Vector3 newOffset = rot * _startOffset;
        Vector3 newPos = _pivot + newOffset;

        _rb.MovePosition(newPos);
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
            EvadeType.Side => transform.forward,
            EvadeType.Flip => transform.right,
            _ => transform.forward
        };
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
        UpdateOrbitMovement();
    }
}
