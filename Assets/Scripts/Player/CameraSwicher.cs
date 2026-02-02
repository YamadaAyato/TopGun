using Unity.Cinemachine;
using UnityEngine;

public class CameraSwicher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _frontCamera;
    [SerializeField] private CinemachineCamera _rearCamera;

    private PlayerInputHandler _inputHandler;

    private void ApplyRearView(bool isRearView)
    {
        _frontCamera.Priority = isRearView ? 0 : 10;
        _rearCamera.Priority = isRearView ? 10 : 0;
    }

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        _inputHandler.RearViewPerformed += ApplyRearView;
    }

    private void OnDisable()
    {
        _inputHandler.RearViewPerformed -= ApplyRearView;
    }
}
