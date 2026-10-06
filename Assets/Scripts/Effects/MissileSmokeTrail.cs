using UnityEngine;

/// <summary>
///     ミサイル再利用時に煙をリセットし、発射位置から発生させるクラス
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class MissileSmokeTrail : MonoBehaviour
{
    private ParticleSystem _smoke;
    private bool _pendingStart;

    private void Awake()
    {
        _smoke = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        _smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _pendingStart = true;
    }

    private void LateUpdate()
    {
        if (!_pendingStart) return;
        // プールから取得した後の発射位置設定を待って開始する。
        _smoke.Clear(true);
        _smoke.Play(true);
        _pendingStart = false;
    }

    private void OnDisable()
    {
        _pendingStart = false;
        if (_smoke != null)
            _smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}