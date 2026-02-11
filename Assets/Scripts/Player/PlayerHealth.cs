using System;
using UnityEngine;

/// <summary>
///     プレイヤーのHP管理をするクラス
/// </summary>
public class PlayerHealth : MonoBehaviour,IDamageable
{
    public event Action<int,int> OnHealthChanged;

    /// <summary>
    /// ダメージを与えられるか返す
    /// 無敵がtrueの時Hitできるため逆を返す
    /// </summary>
    public bool CanBeHit => !_isInvincible;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;

    [SerializeField, ReadOnly,Tooltip("現在無敵がどうか")] private bool _isInvincible;
    [SerializeField, ReadOnly] private int _currentHealth;
    [SerializeField] private int _maxHealth;

    /// <summary> 無敵判定を切り替える </summary>
    /// <param name="value"></param>
    public void SetInvincible(bool value)
    {
        _isInvincible = value;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        Debug.Log($"プレイヤーに{damage}ダメージ、現在HP{_currentHealth}");

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        _currentHealth = 0;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        Debug.Log("プレイヤー死亡");
    }

    private void Awake()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        PlayerLocator.Instance.Register(this);
    }

    private void OnDisable()
    {
        PlayerLocator.Instance.Unregister();
    }
}
