using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public sealed class JustEvadeBlurController : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Volume _volume;

    [Header("Timing")]
    [SerializeField] private float _boostTime = 0.05f;
    [SerializeField] private float _holdTime = 0.05f;
    [SerializeField] private float _returnTime = 0.20f;

    [Header("Blur (Depth Of Field - Gaussian)")]
    [SerializeField, Tooltip("ぼかしの強さ（大きいほど強い）")]
    private float _blurStrength = 1.0f;

    private DepthOfField _dof;
    private Tween _tween;

    // 戻し先（基準値）
    private float _baseStart;
    private float _baseEnd;
    private bool _baseActive;

    private void Awake()
    {
        if (_volume == null) _volume = FindFirstObjectByType<Volume>();
        if (_volume == null || _volume.profile == null)
        {
            Debug.LogError("JustEvadeBlurController: Volume/Profile not found.");
            enabled = false;
            return;
        }

        if (!_volume.profile.TryGet(out _dof) || _dof == null)
        {
            Debug.LogError("JustEvadeBlurController: Depth Of Field override not found in Volume Profile.");
            enabled = false;
            return;
        }

        // Gaussian前提：Start/Endを使ってぼかし量を作る
        _baseActive = _dof.active;

        _baseStart = _dof.gaussianStart.value;
        _baseEnd = _dof.gaussianEnd.value;
    }

    private void OnEnable()
    {
        GameEvents.OnJustEvade += Play;
    }

    private void OnDisable()
    {
        GameEvents.OnJustEvade -= Play;
        _tween?.Kill();
        _tween = null;
        RestoreBase();
    }

    /// <summary>
    /// ジャスト回避時の「視野がぼやける」演出を再生する
    /// （後から別演出を足すときもここに追記していけばOK）
    /// </summary>
    public void Play()
    {
        if (_dof == null) return;

        _tween?.Kill();

        // DoFを有効化（元が無効なら一時的にON）
        _dof.active = true;

        // ぼかしを強くするターゲット
        // Start を手前に、End を手前に寄せると“全体がぼけやすい”
        float targetStart = Mathf.Max(0.01f, _baseStart * 0.25f);
        float targetEnd = Mathf.Max(targetStart + 0.01f, _baseEnd * (1f - 0.35f * _blurStrength));

        _tween = DOTween.Sequence()
            .Append(DOVirtual.Float(_dof.gaussianStart.value, targetStart, _boostTime, v => _dof.gaussianStart.value = v))
            .Join(DOVirtual.Float(_dof.gaussianEnd.value, targetEnd, _boostTime, v => _dof.gaussianEnd.value = v))
            .AppendInterval(_holdTime)
            .Append(DOVirtual.Float(_dof.gaussianStart.value, _baseStart, _returnTime, v => _dof.gaussianStart.value = v).SetEase(Ease.OutQuad))
            .Join(DOVirtual.Float(_dof.gaussianEnd.value, _baseEnd, _returnTime, v => _dof.gaussianEnd.value = v).SetEase(Ease.OutQuad))
            .OnComplete(() =>
            {
                // 元々DoFが無効だったなら戻す
                _dof.active = _baseActive;
            })
            .SetUpdate(true);
    }

    private void RestoreBase()
    {
        if (_dof == null) return;
        _dof.gaussianStart.value = _baseStart;
        _dof.gaussianEnd.value = _baseEnd;
        _dof.active = _baseActive;
    }
}
