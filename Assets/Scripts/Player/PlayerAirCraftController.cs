using UnityEngine;

/// <summary>　プレイヤーの航空機の実態制御をするクラス　</summary>
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAirCraftController : MonoBehaviour
{
    [SerializeField] private float _baseSpeed;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _acceleration;
    
    private PlayerInputHandler _inputHandler;
    private Rigidbody _rb;
    private float _currentSpeed;

    private void FowardMovement()
    {
        _currentSpeed += _inputHandler.Throttle * _acceleration * Time.fixedDeltaTime;
        _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, _maxSpeed);

        Vector3 forwardMovement = transform.forward * _currentSpeed;
        _rb.linearVelocity = forwardMovement;
    }

    private void FixedUpdate()
    {
        FowardMovement();
    }

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _rb = GetComponent<Rigidbody>();
        _currentSpeed = _baseSpeed;
    }
}
