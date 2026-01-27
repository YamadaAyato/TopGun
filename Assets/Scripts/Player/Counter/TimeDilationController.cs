using UnityEngine;

/// <summary>
///     TimeScaleの管理を行うクラス
/// </summary>
public class TimeDilationController : MonoBehaviour
{
    [Tooltip("元の FixedUpdate の間隔を保存する変数")] private float _baseFixedDeltaTime;
    [Tooltip("タイムスケールに影響されないタイマー")] private float _timerUnscaled;
    private float _targetScale;
    private float _durationUnscaled;
    private bool _playing;

    public void Play(float targetScale, float durationUnscaled)
    {
        _targetScale = targetScale;
        _durationUnscaled = durationUnscaled;
        _timerUnscaled = 0f;
        _playing = true;

        ApplyScale(_targetScale);
    }

    private void ApplyScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = _baseFixedDeltaTime * scale;

        Debug.Log($"TimeScale : {Time.timeScale}, FixedDeltaTime : {Time.fixedDeltaTime}");
    }

    private void Awake()
    {
        _baseFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        if (!_playing) return;
        _timerUnscaled += Time.unscaledDeltaTime;
        if (_timerUnscaled >= _durationUnscaled)
        {
            _playing = false;
            ApplyScale(1f);
        }
    }
}
