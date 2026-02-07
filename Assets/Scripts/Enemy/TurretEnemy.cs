using UnityEngine;

/// <summary>
///     単発タレット型の敵クラス
/// </summary>
public class TurretEnemy : TurretEnemyBase
{
    protected override void OnSpawned()
    {
        base.OnSpawned();
    }

    protected override void FireAtPlayer(Transform player)
    {
        _shooter?.Fire(this.transform, player);
    }
}