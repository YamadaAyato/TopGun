using UnityEngine;
using System;

public abstract class EnemyBase : MonoBehaviour, IDamegeble
{
    [SerializeField, ReadOnly] private int _currentHp;
    [SerializeField] private int _maxHp;

    Action<EnemyBase> _onRelease;

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

    public void Die()
    {
        _currentHp = 0;
    }

    protected void Release()
    {
        _onRelease?.Invoke(this);
    }
}
