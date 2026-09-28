using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     ジャスト回避用の近くの銃弾を探知し、保持するクラス
/// </summary>
public class JustEvadeDetector : MonoBehaviour
{
    [SerializeField] private float _justDistance;

    private readonly HashSet<BulletBase> _bullets = new();

    /// <summary>
    ///     有効な弾の中からジャスト回避の距離内にある最も近い弾を取得する
    /// </summary>
    /// <param name="playerPos"> プレイヤーの位置 </param>
    /// <param name="closest"> 検出した最も近い弾 </param>
    /// <returns> 対象の弾が見つかった場合はtrue </returns>
    public bool TryGetClosestBullet(Vector3 playerPos, out BulletBase closest)
    {
        closest = null;
        float bestSqr = float.PositiveInfinity;

        // Pool返却時にTriggerの退出通知が届かなくても、命中済みの弾を除外する。
        _bullets.RemoveWhere(b => b == null || !b.isActiveAndEnabled);

        foreach (var b in _bullets)
        {
            float sqr = (b.transform.position - playerPos).sqrMagnitude;
            if (sqr <= _justDistance * _justDistance && sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = b;
            }
        }
        return closest != null;
    }

    // ======================追加と削除======================

    private void OnDisable()
    {
        _bullets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            if(other.TryGetComponent<BulletBase>(out var bullet))
            {
                _bullets.Add(bullet);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            if (other.TryGetComponent<BulletBase>(out var bullet))
            {
                _bullets.Remove(bullet);
            }
        }
    }
}
