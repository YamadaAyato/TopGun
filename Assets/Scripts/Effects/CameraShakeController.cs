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
    [SerializeField, Tooltip("被弾時のシェイク倍率")] private float _hitGain;
    [SerializeField, Tooltip("爆発シェイク時の倍率")] private float _explosionGain;
    [SerializeField, Tooltip("爆発シェイクの最大の届く距離")] private float _maxDistance;
    [SerializeField] private AnimationCurve _fallOffCurve;

    [Header("回避FOV変化")]
    [SerializeField, Tooltip("回避時にどれだけFOVを増やすか")] private float _evadeFovBoost;
    [SerializeField, Tooltip("広がる時間")] private float _evadeFovInTime;
    [SerializeField, Tooltip("戻す時間")] private float _evadeFovOutTime;
    [SerializeField] private Ease _evadeEase;

    private CinemachineBrain _brain;
    private CinemachineImpulseSource _impulseSource;

    private Tween _fovTween;

    // 連打/カメラ切替でも「戻り先」を固定するためのキャッシュ
    private readonly Dictionary<int, float> _baseFovMap = new();
    private CinemachineCamera _fovOwner;

    /// <summary>
    ///     被弾時のカメラシェイクを再生する
    /// </summary>
    /// <param name="intensity"> シェイク強度 </param>
    public void PlayHit(float intensity)
    {
        if (_impulseSource == null) return;

        // シェイク強度を計算
        float gain = _hitGain * Mathf.Clamp01(intensity);

        var cam = GetMainCamera();
        if (cam == null) return;

        // ランダムな方向にシェイクを発生させる
        //float x = Random.Range(-1f, 1f);
        //float y = Random.Range(-1f, 1f);

        //Vector3 dir = (cam.transform.right * x + cam.transform.up * y).normalized;

        _impulseSource.GenerateImpulse(Vector3.up * gain);
    }

    /// <summary>
    ///     爆発時のカメラシェイクを再生する
    /// </summary>
    /// <param name="explosionPosition"> 爆発のワールド座標 </param>
    public void PlayExplosion(Vector3 explosionPosition)
    {
        if (_impulseSource == null) return;

        var cam = GetMainCamera();
        if (cam == null) return;

        // カメラから爆発位置までの距離
        float distance = Vector3.Distance(cam.transform.position, explosionPosition);

        // 距離を0〜1に正規化し、FallOffカーブで減衰を計算
        float t = Mathf.Clamp01(distance / Mathf.Max(0.001f, _maxDistance));
        float fallOff = Mathf.Clamp01(_fallOffCurve.Evaluate(t));
        float gain = _explosionGain * fallOff;

        // 爆発は上方向 + ランダムな左右方向へのシェイクを発生させる
        Vector3 dir = (cam.transform.up + cam.transform.right * Random.Range(-0.35f, 0.35f)).normalized;

        _impulseSource.GenerateImpulse(dir * gain);
    }

    /// <summary>
    ///     FOV変化のアニメーションを再生する
    /// </summary>
    public void PlayEvadeFovChange()
    {
        // 現在アクティブなCinemachineカメラを取得
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
                () => cmCam.Lens.FieldOfView, // 現在値取得
                fov => SetFov(cmCam, fov), // 値設定
                targetFov,
                _evadeFovInTime
            ).SetEase(Ease.OutQuad))
            .Append(DOTween.To( // ここから基準へ戻す
                () => cmCam.Lens.FieldOfView,
                fov => SetFov(cmCam, fov),
                baseFov,
                _evadeFovOutTime
            ).SetEase(_evadeEase))
            .SetUpdate(true);
    }

    /// <summary>
    ///     現在アクティブなCinemachineカメラを取得する
    /// </summary>
    /// <returns></returns>
    private CinemachineCamera GetActiveCinemachineCamera()
    {
        if (_brain == null) return null;
        return _brain.ActiveVirtualCamera as CinemachineCamera;
    }

    /// <summary>
    ///     メインカメラを取得する
    /// </summary>
    /// <returns></returns>
    private Camera GetMainCamera()
    {
        if (_mainCamera != null) return _mainCamera;
        _mainCamera = Camera.main;
        return _mainCamera;
    }

    /// <summary>
    ///     基準FOVを取得する
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    private float GetBaseFov(CinemachineCamera cam)
    {
        int id = cam.GetInstanceID();
        if (_baseFovMap.TryGetValue(id, out var baseFov)) return baseFov;

        baseFov = cam.Lens.FieldOfView; // 初回だけ記録
        _baseFovMap[id] = baseFov;
        return baseFov;
    }

    /// <summary>
    ///     CinemachineカメラのFOVを設定する
    /// </summary>
    /// <param name="cam"></param>
    /// <param name="fov"></param>
    private void SetFov(CinemachineCamera cam, float fov)
    {
        var lens = cam.Lens;
        lens.FieldOfView = fov;
        cam.Lens = lens;
    }

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
