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

    /// <summary>
    ///     初期化処理を行い、スロー演出を呼ぶ
    /// </summary>
    /// <param name="targetScale"> 適用させるタイムスケールの値 </param>
    /// <param name="durationUnscaled"> 適用させる時間 </param>
    public void Play(float targetScale, float durationUnscaled)
    {
        // 変数への格納とターマーの初期化
        _targetScale = targetScale;
        _durationUnscaled = durationUnscaled;
        _timerUnscaled = 0f;
        _playing = true;

        ApplyScale(_targetScale);
    }

    /// <summary>
    ///     スロー演出の適用をする
    /// </summary>
    /// <param name="scale"> 適用させるタイムスケールの値 </param>
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

        // 適用させる時間を超えたら元に戻す
        if (_timerUnscaled >= _durationUnscaled)
        {
            _playing = false;
            ApplyScale(1f);
        }
    }
}
