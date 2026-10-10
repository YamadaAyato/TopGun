using UnityEngine;

/// <summary>
///     急旋回と回避中だけ翼端の飛行機雲を発生させるクラス
/// </summary>
public class WingtipCondensation : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private PlayerFlightController _aircraft;
    [SerializeField] private PlayerEvadeController _evade;
    [SerializeField] private TrailRenderer[] _trails;
    [SerializeField, Min(0f), Tooltip("飛行機雲が出始める旋回速度（度/秒）")]
    private float _turnThreshold = 20f;
    [SerializeField, Min(0f), Tooltip("飛行機雲を発生させる最低速度")]
    private float _minimumSpeed = 30f;

    [SerializeField, Min(0f), Tooltip("旋回速度の変化へ追従する強さ")]
    private float _turnSmoothing = 12f;
    [SerializeField, Range(0f, 1f), Tooltip("雲を消す旋回速度の比率。開始閾値との差で点滅を防ぐ")]
    private float _turnReleaseRatio = 0.65f;

    private Vector3 _previousForward;
    private float _turnRate;
    private bool _turning;

    private void OnEnable()
    {
        _previousForward = _aircraft != null ? _aircraft.transform.forward : transform.forward;
        _turnRate = 0f;
        _turning = false;
        SetEmission(false);
        ClearTrails();
    }

    private void OnDisable()
    {
        SetEmission(false);
        ClearTrails();
    }

    private void LateUpdate()
    {
        if (_aircraft == null || Time.deltaTime <= 0f) return;
        Vector3 forward = _aircraft.transform.forward;
        float rate = Vector3.Angle(_previousForward, forward) / Time.deltaTime;
        _previousForward = forward;
        _turnRate = Mathf.Lerp(_turnRate, rate, 1f - Mathf.Exp(-_turnSmoothing * Time.deltaTime));
        _turning = _turnRate >= _turnThreshold * (_turning ? _turnReleaseRatio : 1f);
        SetEmission(_aircraft.CurrentSpeed >= _minimumSpeed && (_turning || (_evade != null && _evade.IsEvading)));
    }

    /// <summary>
    ///     再有効化や停止の際に残っている軌跡を消す。
    /// </summary>
    private void ClearTrails()
    {
        foreach (TrailRenderer trail in _trails)
        {
            if (trail != null) trail.Clear();
        }
    }

    /// <summary>
    ///     左右の翼端で雲の発生を切り替える
    /// </summary>
    /// <param name="active"> 発生させるかどうか </param>
    private void SetEmission(bool active)
    {
        foreach (TrailRenderer trail in _trails)
        {
            if (trail != null) trail.emitting = active;
        }
    }
}
