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
    private bool _isDead;

    /// <summary> 現在のHP </summary>
    public int CurrentHealth => _currentHp;
    /// <summary> 最大HP </summary>
    public int MaxHealth => _maxHp;

    /// <summary>
    ///     スポーン時にHPと死亡状態を初期化する
    /// </summary>
    /// <param name="onRelease"> どう戻すかの関数 </param>
    public void Spawn(Action<EnemyBase> onRelease)
    {
        _onRelease = onRelease;
        _currentHp = _maxHp;
        _isDead = false;
        OnSpawned();
    }

    /// <summary>
    ///     ダメージを適用し、HPがなくなった場合は死亡処理をする
    /// </summary>
    /// <param name="damage"> 受けるダメージ量 </param>
    public void TakeDamage(int damage)
    {
        if (_isDead || damage <= 0) return;
        _currentHp -= damage;
        Debug.Log($"{this.name} took {damage} damage. Current HP: {_currentHp}/{_maxHp}");
        if (_currentHp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    ///     爆発とスコア加算を行い、自身を破棄する
    /// </summary>
    public virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;
        _currentHp = 0;
        //Release();
        ProjectileService.Instance.SpawnExplosion(ExplosionType.Big, this.transform);
        ScoreManager.Instance.AddScore(1000,ScorePopupReason.EnemyDown);
        Destroy(gameObject);
    }

    protected virtual void Awake()
    {
        _currentHp = _maxHp;
    }

    /// <summary>
    ///     自身が役目を終えたことを通知し、
    ///     生成時に渡された解放コールバックを呼び出す。
    /// </summary>
    protected void Release()
    {
        _onRelease?.Invoke(this);
    }

    /// <summary>
    ///     スポーン時に派生クラス固有の初期化処理をする
    /// </summary>
    protected virtual void OnSpawned() { }
}
