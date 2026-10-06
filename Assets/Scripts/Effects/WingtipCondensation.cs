using UnityEngine;

/// <summary>
///     急旋回と回避中だけ翼端の飛行機雲を発生させるクラス
/// </summary>
public class WingtipCondensation : MonoBehaviour
{
    [SerializeField] private PlayerAirCraftController _aircraft;
    [SerializeField] private PlayerEvadeController _evade;
    [SerializeField] private TrailRenderer[] _trails;
    [SerializeField, Min(0f), Tooltip("飛行機雲が出始める旋回速度（度/秒）")]
    private float _turnThreshold = 20f;
    [SerializeField, Min(0f), Tooltip("飛行機雲を発生させる最低速度")]
    private float _minimumSpeed = 30f;

    private Vector3 _previousForward;
    private float _turnRate;
    private bool _turning;

    private void OnEnable()
    {
        _previousForward = _aircraft != null ? _aircraft.transform.forward : transform.forward;
        _turnRate = 0f;
        _turning = false;
        SetEmission(false);
        foreach (var trail in _trails)
            if (trail != null) trail.Clear();
    }

    private void OnDisable()
    {
        SetEmission(false);
        foreach (var trail in _trails)
            if (trail != null) trail.Clear();
    }

    private void LateUpdate()
    {
        if (_aircraft == null || Time.deltaTime <= 0f) return;
        Vector3 forward = _aircraft.transform.forward;
        float rate = Vector3.Angle(_previousForward, forward) / Time.deltaTime;
        _previousForward = forward;
        _turnRate = Mathf.Lerp(_turnRate, rate, 1f - Mathf.Exp(-12f * Time.deltaTime));
        _turning = _turnRate >= _turnThreshold * (_turning ? 0.65f : 1f);
        SetEmission(_aircraft.CurrentSpeed >= _minimumSpeed && (_turning || (_evade != null && _evade.IsEvading)));
    }

    /// <summary>
    ///     左右の翼端で雲の発生を切り替える
    /// </summary>
    /// <param name="active"> 発生させるかどうか </param>
    private void SetEmission(bool active)
    {
        foreach (var trail in _trails)
            if (trail != null) trail.emitting = active;
    }
}