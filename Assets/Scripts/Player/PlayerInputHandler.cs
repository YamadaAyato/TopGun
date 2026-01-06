using UnityEngine;
using UnityEngine.InputSystem;

/// <summary> プレイヤーの入力取得をし、状態保持をするクラス </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // 読み取り専用
    public Vector2 Look { get; private set; }
    public float Throttle { get; private set; }
    public float Roll { get; private set; }
    public bool EvadePressed { get; private set; }
    public bool FirePressed { get; private set; }

    private RideActions _rideActions;

    #region Input Callbacks
    private void OnLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();
    private void OnLookCanceled(InputAction.CallbackContext context) => Look = Vector2.zero;
    private void OnThrottle(InputAction.CallbackContext context) => Throttle = context.ReadValue<float>();
    private void OnThrottleCanceled(InputAction.CallbackContext context) => Throttle = 0f;
    private void OnRoll(InputAction.CallbackContext context) => Roll = context.ReadValue<float>();
    private void OnRollCanceled(InputAction.CallbackContext context) => Roll = 0f;
    private void OnEvade(InputAction.CallbackContext context) => EvadePressed = true;
    private void OnEvadeCanceled(InputAction.CallbackContext context) => EvadePressed = false;
    private void OnFire(InputAction.CallbackContext context) => FirePressed = true;
    private void OnFireCanceled(InputAction.CallbackContext context) => FirePressed = false;
    #endregion

    private void Awake()
    {
        _rideActions = new RideActions();
    }

    private void OnEnable()
    {
        _rideActions.Plane.Look.performed += OnLook;
        _rideActions.Plane.Look.canceled += OnLookCanceled;

        _rideActions.Plane.Throttle.performed += OnThrottle;
        _rideActions.Plane.Throttle.canceled += OnThrottleCanceled;

        _rideActions.Plane.Roll.performed += OnRoll;
        _rideActions.Plane.Roll.canceled += OnRollCanceled;

        _rideActions.Plane.Evade.performed += OnEvade;
        _rideActions.Plane.Evade.canceled += OnEvadeCanceled;

        _rideActions.Plane.Fire.performed += OnFire;
        _rideActions.Plane.Fire.canceled += OnFireCanceled;

        _rideActions.Enable();
    }

    private void OnDisable()
    {
        _rideActions.Plane.Look.performed -= OnLook;
        _rideActions.Plane.Look.canceled -= OnLookCanceled;

        _rideActions.Plane.Throttle.performed -= OnThrottle;
        _rideActions.Plane.Throttle.canceled -= OnThrottleCanceled;

        _rideActions.Plane.Roll.performed -= OnRoll;
        _rideActions.Plane.Roll.canceled -= OnRollCanceled;

        _rideActions.Plane.Evade.performed -= OnEvade;
        _rideActions.Plane.Evade.canceled -= OnEvadeCanceled;

        _rideActions.Plane.Fire.performed -= OnFire;
        _rideActions.Plane.Fire.canceled -= OnFireCanceled;

        _rideActions.Disable();
    }
}