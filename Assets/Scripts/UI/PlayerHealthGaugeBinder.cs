using UnityEngine;

/// <summary>
///     HPゲージとプレイヤーの体力情報を紐づけるクラス
/// </summary>
public class PlayerHealthGaugeBinder : MonoBehaviour
{
    [SerializeField] private RadialGaugeView _gaugeView;

    private PlayerHealth _playerHealth;

    /// <summary>
    ///     ゲージ更新
    /// </summary>
    /// <param name="current"></param>
    /// <param name="max"></param>
    private void UpdateGauge(int current, int max)
    {
        _gaugeView.SetNormalized((float)current / max);
    }

    private void Start()
    {
        // AwakeでPlayerLocatorが初期化されるのでここでとってくる
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
