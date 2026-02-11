using System;
using UnityEngine;

/// <summary>
///     回避ゲージを管理するクラス
/// </summary>
public class EvationGauge : MonoBehaviour
{
    /// <summary> 回避のチャージ数が変化したときに発火するイベント / </summary>
    public event Action<float, int> OnChargesChanged;

    /// <summary> 現在のチャージ数  </summary>
    public float CurrentCharges => _currentCharges;
    /// <summary> 最大チャージ数 </summary>
    public int MaxCharges => _maxCharges;
    public bool IsEvading => _isEvading;

    [SerializeField, ReadOnly] private float _currentCharges;
    [SerializeField, Tooltip("最大チャージ数")] private int _maxCharges;
    [SerializeField, Tooltip("1秒あたり何回分回復するか")] private float _regenChargesPerSecond;

    private bool _isEvading;

    /// <summary>
    ///     回避できるかどうかを返し、できるならチャージを1消費する
    /// </summary>
    /// <returns></returns>
    public bool TryConsumeCharge()
    {
        if (_currentCharges >= 1)
        {
            _currentCharges--;
            Raise();
            return true;
        }
        return false;
    }

    /// <summary>
    ///     チャージを回復する
    ///     ジャスト回避時などに呼ばれる
    /// </summary>
    /// <param name="amount"></param>
    public void RecorverCharge(int amount)
    {
        _currentCharges = Mathf.Min(_currentCharges + amount, _maxCharges);
        Raise();
    }

    public void StartEvading()
    {
        _isEvading = true;
    }

    public void StopEvading()
    {
        _isEvading = false;
    }

    private void Raise()
    {
        OnChargesChanged?.Invoke(_currentCharges, _maxCharges);
    }

    private void Awake()
    {
        _currentCharges = _maxCharges;
    }

    private void Update()
    {
        // 回避中でなく、最大チャージ数に達していなければ回復する
        if (_currentCharges < _maxCharges && !_isEvading)
        {
            _currentCharges += _regenChargesPerSecond * Time.deltaTime;
            _currentCharges = Mathf.Min(_currentCharges, _maxCharges);

            Raise();
        }
    }
}
