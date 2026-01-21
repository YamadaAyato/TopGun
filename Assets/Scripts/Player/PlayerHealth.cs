using UnityEngine;

/// <summary>
///     プレイヤーのHP関係の処理をするクラス
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    /// <summary>
    /// ダメージを与えられるか返す
    /// 無敵がtrueの時Hitできるため逆を返す
    /// </summary>
    public bool CanBeHit => !_isInvincible;

    [SerializeField, ReadOnly,Tooltip("現在無敵がどうか")] private bool _isInvincible;
    [SerializeField, ReadOnly] private int _currentHealth;
    [SerializeField] private int _health;

    /// <summary> ダメージ処理をする。 </summary>
    /// <param name="damage"> ダメージ </param>
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
