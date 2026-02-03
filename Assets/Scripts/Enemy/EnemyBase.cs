using UnityEngine;
using System;

/// <summary>
///     敵の基底クラス
/// </summary>
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("HP設定")]
    [SerializeField, ReadOnly] private int _currentHp;
    [SerializeField] private int _maxHp;

     private Action<EnemyBase> _onRelease;

    /// <summary> スポーン時の初期化処理をする </summary>
    /// <param name="onRelease"> どう戻すかの関数 </param>
    public void Spawn(Action<EnemyBase> onRelease)
    {
        _onRelease = onRelease;
        _currentHp = _maxHp;
    }

    public void TakeDamage(int damage)
    {
        _currentHp -= damage;

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        _currentHp = 0;
    }

    /// <summary>
    ///     自身が役目を終えたことを通知し、
    ///     生成時に渡された解放コールバックを呼び出す。
    /// </summary>
    protected void Release()
    {
        _onRelease?.Invoke(this);
    }

    /// <summary> スポーン時のイベント </summary>
    protected virtual void OnSpawned() { }
}
