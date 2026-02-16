using System;
using UnityEngine;

/// <summary>
///     ステージシーン内での制限時間の管理を行うクラス
/// </summary>
public class StageCountDownTimer : MonoBehaviour
{
    public event Action<float> OnRemainingTimeChanged;
    public event Action OnTimeUp;

    [SerializeField, Tooltip("ステージの制限時間")] private float _stageTime;

    private float _remainingTime;
    private bool _isCountingDown;
    private bool _isfinished;

    /// <summary>
    ///     カウントダウンを開始する
    /// </summary>
    public void StartCountDown()
    {
        _remainingTime = _stageTime;

        _isCountingDown = true;
        _isfinished = false;
        RaiseChanged();
    }

    /// <summary>
    ///     カウントダウンを停止する
    /// </summary>
    public void StopCountDown()
    {
        _isCountingDown = false;
    }

    /// <summary>
    ///     カウントダウンを再開する
    /// </summary>
    public void ResumeTimer()
    {
        if (_isfinished) return;
        _isCountingDown = true;
    }

    /// <summary>
    ///     カウントダウンをリセットする
    /// </summary>
    public void ResetCountDown()
    {
        _isfinished = false;
        _isCountingDown = false;

        _remainingTime = _stageTime;
        RaiseChanged();
    }

    /// <summary>
    ///     時間の変化を通知する
    /// </summary>
    private void RaiseChanged()
    {
        OnRemainingTimeChanged?.Invoke(_remainingTime);
    }

    private void Start()
    {
        ResetCountDown();
        StartCountDown();
    }

    private void Update()
    {
        // カウントダウンが終了しているか、停止している場合は処理しない
        if (!_isCountingDown) return;
        if (_isfinished) return;

        // 時間を減らす
        _remainingTime -= Time.unscaledDeltaTime;
        RaiseChanged();

        // 時間が0以下になったら、カウントダウンを終了する
        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            _isfinished = true;
            _isCountingDown = false;

            RaiseChanged();
            OnTimeUp?.Invoke();
        }
    }
}
