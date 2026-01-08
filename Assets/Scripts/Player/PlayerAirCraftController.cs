using UnityEngine;

/// <summary> プレイヤーの航空機の実態制御をするクラス </summary>
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerAirCraftController : MonoBehaviour
{
    [Header("表示用")]
    [ReadOnly, SerializeField] private float _currentSpeed;

    [Header("前進移動の設定")]
    [SerializeField] private float _baseSpeed;
    [SerializeField] private float _minSpeed;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _acceleration;

    [Header("回転の設定")]
    [SerializeField] private float _pitchSpeed;
    [SerializeField] private float _yawSpeed;
    [SerializeField] private float _rollSpeed;

    [Header("角度の制限")]
    [SerializeField] private float _maxPitchAngle;
    [SerializeField] private float _maxRollAngle;

    [SerializeField] private float _stabilizeSpeed;
    [SerializeField] private float _rollYawFactor;

    private PlayerInputHandler _inputHandler;
    private Rigidbody _rb;

    /// <summary>
    ///     前進移動の処理をする
    ///     加速や減速の入力を受け取り、Rigidbodyの速度を更新する
    /// </summary>
    private void FowardMovement()
    {
        // 入力からの速度を計算して更新
        _currentSpeed += _inputHandler.Throttle * _acceleration * Time.fixedDeltaTime;
        _currentSpeed = Mathf.Clamp(_currentSpeed, _minSpeed, _maxSpeed);

        Vector3 forwardMovement = transform.forward * _currentSpeed;
        _rb.linearVelocity = forwardMovement;
    }

    /// <summary>
    ///     機体回転や旋回を処理する
    ///     回転や機首制御の入力を受け取り、Rigidbodyの回転を更新する
    /// </summary>
    private void Rotation()
    {
        Vector2 lookInput = _inputHandler.Look;
        float rollInput = _inputHandler.Roll;

        // 入力からの回転量を計算(それぞれの軸回転量を算出)
        // pitch：機首の上下回転
        // yaw：左右旋回
        // roll：傾き回転
        float pitch = lookInput.y * _pitchSpeed * Time.fixedDeltaTime;
        float yaw = lookInput.x * _yawSpeed * Time.fixedDeltaTime;
        float roll = rollInput * _rollSpeed * Time.fixedDeltaTime;

        // 現在の角度をそれぞれ取得
        float currentPitch = NormalizeAngle(_rb.rotation.eulerAngles.x);
        float currentYaw = NormalizeAngle(_rb.rotation.eulerAngles.y);
        float currentRoll = NormalizeAngle(_rb.rotation.eulerAngles.z);

        // ピッチとロールが最大角度を超えないように制限
        if (currentPitch + pitch > _maxPitchAngle || currentPitch + pitch < -_maxPitchAngle)
            pitch = 0f;
        if (currentRoll + roll > _maxRollAngle || currentRoll + roll < -_maxRollAngle)
            roll = 0f;

        // ロールに応じてヨー回転も追加する
        // SignedAngleは2つのベクトル間の符号(+.-)付きの角度を返す
        float rollAngle = Vector3.SignedAngle(transform.up, Vector3.up, transform.forward);
        float rollYaw = rollAngle * _rollYawFactor * (_currentSpeed / _maxSpeed) * Time.fixedDeltaTime;

        // 回転の差分を計算してRigidbodyに適用
        Quaternion deltaRotation = Quaternion.Euler(pitch, yaw + rollYaw, -roll);
        _rb.MoveRotation(_rb.rotation * deltaRotation);

        // 入力がないなら姿勢を通常状態に戻す
        if (Mathf.Approximately(rollInput, 0f))
        {
            Quaternion target = Quaternion.Euler(currentPitch, currentYaw, 0f);

            // Lerpは回転角度が大きいと不均一な速度になるのでSlerpを使用
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, target, _stabilizeSpeed * Time.fixedDeltaTime));
        }
    }

    /// <summary>
    ///     角度を-180〜180度の範囲に正規化する
    ///     180度を超える場合は-の角度に変換する
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }
        return angle;
    }

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _rb = GetComponent<Rigidbody>();
        _currentSpeed = _baseSpeed;
    }

    private void FixedUpdate()
    {
        FowardMovement();
        Rotation();
    }
}
