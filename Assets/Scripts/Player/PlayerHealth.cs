using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField, ReadOnly,Tooltip("現在無敵がどうか")] private bool _isInvincible;
    [SerializeField, ReadOnly] private int _currentHealth;
    [SerializeField] private int _health;

    public void TakeDamage(int damage)
    {
        _health -= damage;
        Debug.Log($"プレイヤーに{damage}ダメージ、現在HP{_health}");

        if(_health <= 0)
        {
            // TODO:死亡時処理
            _health = 0;
            Debug.Log("プレイヤー死亡");
        }
    }
}
