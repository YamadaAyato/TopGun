using Unity.Cinemachine;
using UnityEngine;

/// <summary>
///     カメラを前方視点と後方視点で切り替えるクラス
/// </summary>
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "CameraSwicher")]
public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _frontCamera;
    [SerializeField] private CinemachineCamera _rearCamera;

    private PlayerInputHandler _inputHandler;

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

    /// <summary>
    ///     カメラを前方視点・後方視点で切り替える
    /// </summary>
    /// <param name="isRearView"> trueで後方カメラへ </param>
    private void ApplyRearView(bool isRearView)
    {
        _frontCamera.Priority = isRearView ? 0 : 10;
        _rearCamera.Priority = isRearView ? 10 : 0;
    }
}
