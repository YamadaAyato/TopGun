using System;
using UnityEngine;

public class StageCountDownTimer : MonoBehaviour
{
    public event Action<float> OnRemainingTimeChanged;
    public event Action OnTimeUp;

    [SerializeField, Tooltip("ステージの制限時間")] private float _stageTime;

    private float _remainingTime;
    private bool _isCountingDown;
    private bool _isfinished;

    public void StartCountDown()
    {
        _remainingTime = _stageTime;

        _isCountingDown = true;
        _isfinished = false;
        RaiseChanged();
    }

    public void StopCountDown()
    {
        _isCountingDown = false;
    }

    public void ResumeTimer()
    {
        if (_isfinished) return;
        _isCountingDown = true;
    }

    public void ResetCountDown()
    {
        _isfinished = false;
        _isCountingDown = false;

        _remainingTime = _stageTime;
        RaiseChanged();
    }

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
        if (!_isCountingDown) return;
        if (_isfinished) return;

        _remainingTime -= Time.unscaledDeltaTime;
        RaiseChanged();

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
