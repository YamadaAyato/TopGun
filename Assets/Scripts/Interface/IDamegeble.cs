/// <summary> ダメージ関係の処理をする </summary>
public interface IDamegeble
{
    /// <summary> ダメージ処理をする </summary>
    /// <param name="damage"> ダメージ </param>
    void TakeDamage(int damage);

    /// <summary> 死亡時処理をする </summary>
    void Die();
}
