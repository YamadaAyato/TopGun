using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
///     タイトルシーン内での入力を管理するクラス
/// </summary>
public class TitleInputHandler : MonoBehaviour
{
    public event Action OnClicked;
    private RideActions _rideActions;

    /// <summary>
    ///     クリック時に呼び出される関数
    /// </summary>
    /// <param name="context"></param>
    private void HandleClick(InputAction.CallbackContext context)
    {
        OnClicked?.Invoke();
    }

    private void Awake()
    {
        _rideActions = new RideActions();
    }

    private void OnEnable()
    {
        _rideActions.Enable();
        _rideActions.Title.Click.performed += HandleClick;
    }

    private void OnDisable()
    {
        _rideActions.Title.Click.performed -= HandleClick;
        _rideActions.Disable();
    }
}
