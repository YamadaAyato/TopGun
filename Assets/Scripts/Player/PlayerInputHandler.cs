using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 Look { get; private set; }
    public float Throttle { get; private set; }
    public float Roll { get; private set; }
    public bool EvadePressed { get; private set; }
    public bool FirePressed { get; private set; }


    private RideActions _rideActions;

    private void Awake()
    {
        _rideActions = new RideActions();
    }

    private void OnEnable()
    {
        _rideActions.Plane.Look.performed += c => Look = c.ReadValue<Vector2>();
        _rideActions.Plane.Look.canceled += _ => Look = Vector2.zero;

        _rideActions.Plane.Throttle.performed += c => Throttle = c.ReadValue<float>();
        _rideActions.Plane.Throttle.canceled += _ => Throttle = 0f;

        _rideActions.Plane.Roll.performed += c => Roll = c.ReadValue<float>();
        _rideActions.Plane.Roll.canceled += _ => Roll = 0f;

        _rideActions.Plane.Evade.performed += _ => EvadePressed = true;
        _rideActions.Plane.Evade.canceled += _ => EvadePressed = false;

        _rideActions.Plane.Fire.performed += _ => FirePressed = true;
        _rideActions.Plane.Fire.canceled += _ => FirePressed = false;

        _rideActions.Enable();
    }

    private void OnDisable()
    {
        _rideActions.Plane.Look.performed -= c => Look = c.ReadValue<Vector2>();
        _rideActions.Plane.Look.canceled -= _ => Look = Vector2.zero;

        _rideActions.Plane.Throttle.performed -= c => Throttle = c.ReadValue<float>();
        _rideActions.Plane.Throttle.canceled -= _ => Throttle = 0f;

        _rideActions.Plane.Roll.performed -= c => Roll = c.ReadValue<float>();
        _rideActions.Plane.Roll.canceled -= _ => Roll = 0f;

        _rideActions.Plane.Evade.performed -= _ => EvadePressed = true;
        _rideActions.Plane.Evade.canceled -= _ => EvadePressed = false;

        _rideActions.Plane.Fire.performed -= _ => FirePressed = true;
        _rideActions.Plane.Fire.canceled -= _ => FirePressed = false;

        _rideActions.Disable();
    }

    private void OnDestroy()
    {
        _rideActions.Plane.Look.performed -= c => Look = c.ReadValue<Vector2>();
        _rideActions.Plane.Look.canceled -= _ => Look = Vector2.zero;

        _rideActions.Plane.Throttle.performed -= c => Throttle = c.ReadValue<float>();
        _rideActions.Plane.Throttle.canceled -= _ => Throttle = 0f;

        _rideActions.Plane.Roll.performed -= c => Roll = c.ReadValue<float>();
        _rideActions.Plane.Roll.canceled -= _ => Roll = 0f;

        _rideActions.Plane.Evade.performed -= _ => EvadePressed = true;
        _rideActions.Plane.Evade.canceled -= _ => EvadePressed = false;

        _rideActions.Plane.Fire.performed -= _ => FirePressed = true;
        _rideActions.Plane.Fire.canceled -= _ => FirePressed = false;

        _rideActions.Disable();
    }
}
