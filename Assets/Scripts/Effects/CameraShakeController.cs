using DG.Tweening;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
///     Cinemachineを用いたカメラシェイクやFOV変化を制御するクラス
///     Hit時のシェイク、爆発時のシェイク、回避時のFOV変化をする
/// </summary>
public class CameraShakeController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Camera _mainCamera;

    [Header("Hit時や爆発時の揺れの設定")]
    [SerializeField] private float _hitGain;
    [SerializeField] private float _explosionGain;
    [SerializeField] private float _maxDistance;
    [SerializeField] private AnimationCurve _fallOffCurve;

    [Header("回避FOC変化")]
    [SerializeField] private float _evadeFovBoost;
    [SerializeField] private float _evadeFovInTime;
    [SerializeField] private float _evadeFovOutTime;
    [SerializeField] private Ease _evadeEase;

    private CinemachineBrain _brain;
    private CinemachineImpulseSource _impulseSource;

    private Tween _fovTween;

    // 連打/カメラ切替でも「戻り先」を固定するためのキャッシュ
    private readonly Dictionary<int, float> _baseFovMap = new();
    private CinemachineCamera _fovOwner;

    public void PlayHit(float intensity)
    {
        if (_impulseSource == null) return;

        float gain = _hitGain * Mathf.Clamp01(intensity);

        var cam = GetMainCamera();
        if (cam == null) return;

        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);

        Vector3 dir = (cam.transform.right * x + cam.transform.up * y).normalized;

        _impulseSource.GenerateImpulse(dir * gain);
    }

    public void PlayExplosion(Vector3 explosionPosition)
    {
        if (_impulseSource == null) return;

        var cam = GetMainCamera();
        if (cam == null) return;

        float distance = Vector3.Distance(cam.transform.position, explosionPosition);
        float t = Mathf.Clamp01(distance / Mathf.Max(0.001f, _maxDistance));
        float fallOff = Mathf.Clamp01(_fallOffCurve.Evaluate(t));
        float gain = _explosionGain * fallOff;

        if (gain <= 0.001f) return;

        // 爆発は「上＋少し横でやる
        Vector3 dir = (cam.transform.up + cam.transform.right * Random.Range(-0.35f, 0.35f)).normalized;

        _impulseSource.GenerateImpulse(dir * gain);
    }

    public void PlayEvadeFovChange()
    {
        var cmCam = GetActiveCinemachineCamera();
        if (cmCam == null) return;

        // 連打対策
        // 前のTweenをKillする前に「前回いじってたカメラ」を基準FOVへ戻しておく
        if (_fovTween != null && _fovTween.IsActive())
        {
            _fovTween.Kill();
            if (_fovOwner != null)
            {
                float basePrev = GetBaseFov(_fovOwner);
                SetFov(_fovOwner, basePrev);
            }
        }

        _fovOwner = cmCam;

        float baseFov = GetBaseFov(cmCam);
        float targetFov = baseFov + _evadeFovBoost;

        // 現在値→ブースト→基準へ戻す
        _fovTween = DOTween.Sequence()
            .Append(DOTween.To(
                () => cmCam.Lens.FieldOfView,
                fov => SetFov(cmCam, fov),
                targetFov,
                _evadeFovInTime
            ).SetEase(Ease.OutQuad))
            .Append(DOTween.To(
                () => cmCam.Lens.FieldOfView,
                fov => SetFov(cmCam, fov),
                baseFov,
                _evadeFovOutTime
            ).SetEase(_evadeEase))
            .SetUpdate(true);
    }

    private CinemachineCamera GetActiveCinemachineCamera()
    {
        if (_brain == null) return null;
        return _brain.ActiveVirtualCamera as CinemachineCamera;
    }

    private Camera GetMainCamera()
    {
        if (_mainCamera != null) return _mainCamera;
        _mainCamera = Camera.main;
        return _mainCamera;
    }

    private float GetBaseFov(CinemachineCamera cam)
    {
        int id = cam.GetInstanceID();
        if (_baseFovMap.TryGetValue(id, out var baseFov)) return baseFov;

        baseFov = cam.Lens.FieldOfView; // 初回だけ記録
        _baseFovMap[id] = baseFov;
        return baseFov;
    }

    private void SetFov(CinemachineCamera cam, float fov)
    {
        var lens = cam.Lens;
        lens.FieldOfView = fov;
        cam.Lens = lens;
    }

    // --------------------
    // Unity lifecycle
    // --------------------

    private void Awake()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        if (_mainCamera == null) _mainCamera = Camera.main;
        _brain = _mainCamera != null ? _mainCamera.GetComponent<CinemachineBrain>() : null;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerHit += PlayHit;
        GameEvents.OnExplosion += PlayExplosion;
        GameEvents.OnEvade += PlayEvadeFovChange;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerHit -= PlayHit;
        GameEvents.OnExplosion -= PlayExplosion;
        GameEvents.OnEvade -= PlayEvadeFovChange;

        _fovTween?.Kill();
        _fovTween = null;

        // 念のため、無効化時に前回弄ってたカメラを基準に戻す
        if (_fovOwner != null)
        {
            SetFov(_fovOwner, GetBaseFov(_fovOwner));
            _fovOwner = null;
        }
    }
}
