using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
///     ジャスト回避中の画面を白黒にし、フレア爆発の色だけ残すクラス
/// </summary>
public class JustEvadeMonochrome : MonoBehaviour
{
    [SerializeField, Tooltip("白黒にする主カメラ")] private Camera _camera;
    [SerializeField, Tooltip("演出終了のタイミングを参照する時間制御")] private TimeDilationController _timeDilation;
    [SerializeField, Tooltip("色を残す爆発専用のレイヤー番号")] private int _explosionLayer = 30;
    [SerializeField, Tooltip("元の色へ戻す時間（秒）")] private float _fadeOutDuration = 0.3f;
    [SerializeField, Tooltip("爆発だけを色付きで重ねるカメラ")] private Camera _explosionCamera;

    [SerializeField, Tooltip("白黒演出のVolume優先度。他の画面演出との重なりに合わせて調整する")]
    private float _volumePriority = 1000f;

    private UniversalAdditionalCameraData _cameraData;
    private Volume _volume;
    private VolumeProfile _profile;
    private int _originalMask;
    private bool _originalPost;
    private bool _active;

    private const float MONOCHROME_SATURATION = -100f;
    private const float MIN_FADE_DURATION = 0.01f;

    private void Awake()
    {
        _cameraData = _camera.GetUniversalAdditionalCameraData();
        _originalMask = _camera.cullingMask;
        _originalPost = _cameraData.renderPostProcessing;
        var volumeObject = new GameObject("JustEvadeMonochromeVolume");
        volumeObject.transform.SetParent(transform, false);
        volumeObject.layer = gameObject.layer;
        _volume = volumeObject.AddComponent<Volume>();
        _volume.isGlobal = true;
        _volume.priority = _volumePriority;
        _volume.weight = 0;
        _profile = ScriptableObject.CreateInstance<VolumeProfile>();
        _profile.Add<ColorAdjustments>().saturation.Override(MONOCHROME_SATURATION);
        _volume.sharedProfile = _profile;

        _explosionCamera.CopyFrom(_camera);
        _explosionCamera.cullingMask = 1 << _explosionLayer;
        UniversalAdditionalCameraData data = _explosionCamera.GetUniversalAdditionalCameraData();
        data.renderType = CameraRenderType.Overlay;

        data.renderPostProcessing = false;
    }

    private void OnEnable()
    {
        _camera.cullingMask &= ~(1 << _explosionLayer);
        _cameraData.renderPostProcessing = true;
        _explosionCamera.enabled = true;
        _cameraData.cameraStack.Add(_explosionCamera);
        GameEvents.OnJustEvade += HandleJustEvade;
        GameEvents.OnFlareExplosion += HandleFlareExplosion;
    }

    private void LateUpdate()
    {
        _explosionCamera.fieldOfView = _camera.fieldOfView;
        _explosionCamera.nearClipPlane = _camera.nearClipPlane;
        _explosionCamera.farClipPlane = _camera.farClipPlane;
        _explosionCamera.projectionMatrix = _camera.projectionMatrix;
        if (!_active) return;
        if (_timeDilation != null && _timeDilation.IsPlaying) return;
        _volume.weight = Mathf.MoveTowards(_volume.weight, 0,
            Time.unscaledDeltaTime / Mathf.Max(MIN_FADE_DURATION, _fadeOutDuration));
        if (_volume.weight <= 0) _active = false;
    }

    private void OnDisable()
    {
        GameEvents.OnJustEvade -= HandleJustEvade;
        GameEvents.OnFlareExplosion -= HandleFlareExplosion;
        if (_volume != null) _volume.weight = 0;
        _active = false;

        foreach (SelectiveExplosionColor effect in FindObjectsByType<SelectiveExplosionColor>(FindObjectsSortMode.None))
        {
            effect.Restore();
        }

        if (_camera == null) return;
        _camera.cullingMask = _originalMask;
        _cameraData.renderPostProcessing = _originalPost;
        _cameraData.cameraStack.Remove(_explosionCamera);
        if (_explosionCamera != null) _explosionCamera.enabled = false;
    }

    private void OnDestroy()
    {
        if (_profile != null) Destroy(_profile);
    }

    /// <summary>
    ///     ジャスト回避が成功した瞬間に白黒へ切り替える
    /// </summary>
    private void HandleJustEvade()
    {
        _active = true;
        _volume.weight = 1;
    }

    /// <summary>
    ///     フレアの迎撃と誘爆だけを色付きで描画する
    /// </summary>
    private void HandleFlareExplosion(ExplosionFx explosion)
    {
        if (explosion == null) return;
        if (!explosion.TryGetComponent(out SelectiveExplosionColor effect))
        {
            effect = explosion.gameObject.AddComponent<SelectiveExplosionColor>();
        }

        effect.Apply(_explosionLayer);
    }
}
