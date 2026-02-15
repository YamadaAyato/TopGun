using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class TitleInputHandler : MonoBehaviour
{
    public event Action OnClicked;
    private RideActions _rideActions;

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
