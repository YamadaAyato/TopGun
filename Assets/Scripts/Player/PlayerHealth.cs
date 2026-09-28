using System;
using UnityEngine;

/// <summary>
///     プレイヤーのHP管理をするクラス
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    /// <summary> HP変化時のイベント </summary>
    public event Action<int, int> OnHealthChanged;
    /// <summary> 死亡時に一度だけ発火するイベント </summary>
    public event Action OnDied;

    /// <summary>
    ///     無敵でも死亡状態でもない場合、ダメージを受けられることを返す
    /// </summary>
    public bool CanBeHit => !_isInvincible && !_isDead;
    /// <summary> 死亡しているか </summary>
    public bool IsDead => _isDead;
    /// <summary> 現在のHP </summary>
    public int CurrentHealth => _currentHealth;
    /// <summary> 最大HP </summary>
    public int MaxHealth => _maxHealth;

    [SerializeField, ReadOnly, Tooltip("現在無敵がどうか")] private bool _isInvincible;
    [SerializeField, ReadOnly] private int _currentHealth;
    [SerializeField] private int _maxHealth;
    private bool _isDead;

    /// <summary>
    ///     無敵判定を切り替える
    /// </summary>
    /// <param name="value"> 無敵状態にする場合はtrue </param>
    public void SetInvincible(bool value)
    {
        _isInvincible = value;
    }

    /// <summary>
    ///     ダメージを適用し、HPの変化や死亡を通知する
    /// </summary>
    /// <param name="damage"> 受けるダメージ量 </param>
    public void TakeDamage(int damage)
    {
        if (!CanBeHit || damage <= 0) return;
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        float intensity = Mathf.Clamp01((float)damage / 30f);
        GameEvents.RaisePlayerHit(intensity);

        Debug.Log($"プレイヤーに{damage}ダメージ、現在HP{_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }

    /// <summary>
    ///     HPを0にし、死亡イベントを一度だけ発火する
    /// </summary>
    public void Die()
    {
        if (_isDead) return;
        _isDead = true;
        _currentHealth = 0;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        Debug.Log("プレイヤー死亡");
        OnDied?.Invoke();
    }

    private void Awake()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        PlayerLocator.Ensure().Register(this);
    }

    private void OnDisable()
    {
        if (PlayerLocator.Instance != null && PlayerLocator.Instance.PlayerHealth == this)
            PlayerLocator.Instance.Unregister();
    }
}
