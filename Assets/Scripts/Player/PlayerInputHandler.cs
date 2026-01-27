using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary> プレイヤーの入力取得をし、状態保持をするクラス </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // 読み取り専用
    public Vector2 Look { get; private set; }
    public float Throttle { get; private set; }
    public float Roll { get; private set; }
    public bool FirePressed { get; private set; }

    public event Action FirePerformed;

    private RideActions _rideActions;

    // Q/Eキーの回避入力の方向管理用(-1: Qキー押下, 0: 押下なし, 1: Eキー押下)
    private int _sideEvadeDirPressed;

    private int _sideEvadeFrame;

    // Spaceキーの回避入力のフレーム管理用(押下されたフレームを保持)
    private int _flipEvadeFrame = -1;

    /// <summary>
    ///     そのフレームで押されていたら、方向付きで左右回避入力を消費する
    /// </summary>
    /// <returns></returns>
    public int ConsumeSideEvadeInput()
    {
        if(_sideEvadeFrame != Time.frameCount)
        {
            // 押されたフレームでなければ0を返す
            return 0;
        }

        // 消費したらリセット
        _sideEvadeFrame = -1;
        int dir = _sideEvadeDirPressed;
        _sideEvadeDirPressed = 0;
        return dir;
    }

    /// <summary>
    ///     宙返り回避が押されたフレームかどうかを判定し、消費する
    /// </summary>
    /// <returns></returns>
    public bool ConsumeFlipEvadeInput()
    {
        if (_flipEvadeFrame == Time.frameCount)
        {
            // 消費したらリセット
            _flipEvadeFrame = -1;
            return true;
        }
        return false;
    }

    #region Input Callbacks
    private void OnLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();
    private void OnLookCanceled(InputAction.CallbackContext context) => Look = Vector2.zero;
    private void OnThrottle(InputAction.CallbackContext context) => Throttle = context.ReadValue<float>();
    private void OnThrottleCanceled(InputAction.CallbackContext context) => Throttle = 0f;
    private void OnRoll(InputAction.CallbackContext context) => Roll = context.ReadValue<float>();
    private void OnRollCanceled(InputAction.CallbackContext context) => Roll = 0f;
    private void OnFire(InputAction.CallbackContext context)
    {
        FirePressed = true;
        FirePerformed?.Invoke();
    }
    private void OnFireCanceled(InputAction.CallbackContext context) => FirePressed = false;

    /// ===========これより下は押した瞬間のみ保持するもの==========

    /// <summary>
    ///     左右回避の押した瞬間を方向付きで保持する
    ///     Q : -1/E : 1
    /// </summary>
    /// <param name="context"></param>
    private void OnSideEvade(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        // 0以外の値が来たら方向をセット
        int dir = value > 0 ? 1 : (value < 0 ? -1 : 0);

        if (dir != 0)
        {
            _sideEvadeDirPressed = dir;
           _sideEvadeFrame = Time.frameCount;
        }
    }

    /// <summary>
    ///     宙返り回避の押した瞬間をフレームで保持する
    /// </summary>
    /// <param name="context"></param>
    private void OnFlipEvade(InputAction.CallbackContext context)
    {
        _flipEvadeFrame = Time.frameCount;
    }
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

        _rideActions.Plane.Fire.performed += OnFire;
        _rideActions.Plane.Fire.canceled += OnFireCanceled;

        _rideActions.Plane.Evade.performed += OnSideEvade;
        _rideActions.Plane.FlipEvade.performed += OnFlipEvade;

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

        _rideActions.Plane.Fire.performed -= OnFire;
        _rideActions.Plane.Fire.canceled -= OnFireCanceled;

        _rideActions.Plane.Evade.performed -= OnSideEvade;
        _rideActions.Plane.FlipEvade.performed -= OnFlipEvade;

        _rideActions.Disable();
    }
}