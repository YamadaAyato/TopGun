using UnityEngine;

public class PlayerHealthGaugeBinder : MonoBehaviour
{
    [SerializeField] private RadialGaugeView _gaugeView;

    private PlayerHealth _playerHealth;

    private void UpdateGauge(int current, int max)
    {
        _gaugeView.SetNormalized((float)current / max);
    }

    private void Start()
    {
        if (_playerHealth == null)
        {
            _playerHealth = PlayerLocator.Instance.PlayerHealth;
            _playerHealth.OnHealthChanged += UpdateGauge;
            UpdateGauge(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        _playerHealth.OnHealthChanged -= UpdateGauge;
    }
}
