using System;
using UnityEngine;

/// <summary>
///     防空圏に一定時間滞在したプレイヤーの失敗を通知するクラス
/// </summary>
public class AirDefenseZone : MonoBehaviour
{
    /// <summary> 防空圏の滞在時間を超えた時のイベント </summary>
    public event Action OnTimeExceeded;

    [SerializeField] private Transform _player;
    [SerializeField, Tooltip("防空圏が始まるワールド座標の高度")]
    private float _altitudeLimit = 200f;
    [SerializeField, Min(0.1f), Tooltip("防空圏に連続して滞在できる秒数")]
    private float _graceSeconds = 3f;

    private float _elapsedSeconds;
    private bool _hasFailed;

    private void OnEnable()
    {
        _elapsedSeconds = 0f;
        _hasFailed = false;
    }

    private void Update()
    {
        if (_player == null || _hasFailed || Time.timeScale <= 0f) return;

        CheckAltitude(_player.position.y, Time.unscaledDeltaTime);
    }

    /// <summary>
    ///     防空圏の連続滞在を計測し、降下した場合は猶予をリセットする
    /// </summary>
    /// <param name="altitude"> プレイヤーの高度 </param>
    /// <param name="deltaTime"> 経過秒数 </param>
    private void CheckAltitude(float altitude, float deltaTime)
    {
        if (_hasFailed) return;
        if (altitude < _altitudeLimit)
        {
            _elapsedSeconds = 0f;
            return;
        }

        _elapsedSeconds += deltaTime;
        if (_elapsedSeconds < _graceSeconds) return;

        _hasFailed = true;
        OnTimeExceeded?.Invoke();
    }
}