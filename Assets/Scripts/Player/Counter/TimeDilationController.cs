using UnityEngine;

public class TimeDilationController : MonoBehaviour
{
    private float _baseFixedDeltaTime;
    private bool _playing;
    private float _timerUnscaled;

    private float _targetScale;
    private float _durationUnscaled;

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
