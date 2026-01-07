using UnityEngine;

/// <summary>　プレイヤーの航空機の実態制御をするクラス　</summary>
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAirCraftController : MonoBehaviour
{
    [Header("前進移動の設定")]
    [SerializeField] private float _baseSpeed;
    [SerializeField] private float _minSpeed;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _acceleration;
    
    private PlayerInputHandler _inputHandler;
    private Rigidbody _rb;
    private float _currentSpeed;

    /// <summary>
    ///         前進移動の処理をするクラス
    ///         加速や減速の入力を受け取り、Rigidbodyの速度を更新する
    /// </summary>
    private void FowardMovement()
    {
        _currentSpeed += _inputHandler.Throttle * _acceleration * Time.fixedDeltaTime;
        _currentSpeed = Mathf.Clamp(_currentSpeed, _minSpeed, _maxSpeed);

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
