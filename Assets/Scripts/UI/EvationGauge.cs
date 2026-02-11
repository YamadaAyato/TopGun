using System;
using UnityEngine;

public class EvationGauge : MonoBehaviour
{
    public event Action<float, int> OnChargesChanged;

    public int MaxCharges => _maxCharges;
    public float CurrentCharges => _currentCharges;
    public bool IsEvading => _isEvading;

    [SerializeField] private int _maxCharges;

    [SerializeField] private float _regenChargesPerSecond;
    [SerializeField, ReadOnly] private float _currentCharges;

    private bool _isEvading;

    public bool TryConsumeCharge()
    {
        if (_currentCharges > 0)
        {
            _currentCharges--;
            Raise();
            return true;
        }
        return false;
    }

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
        if (_currentCharges < _maxCharges && !_isEvading)
        {
            _currentCharges += _regenChargesPerSecond * Time.deltaTime;
            _currentCharges = Mathf.Min(_currentCharges, _maxCharges);

            Raise();
        }
    }
}
