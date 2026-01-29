using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     ジャスト回避用の近くの銃弾を探知し、保持するクラス
/// </summary>
public class JustEvadeDetector : MonoBehaviour
{
    [SerializeField] private float _justDistance;

    private readonly HashSet<BulletBase> _bullets = new();


    public bool TryGetClosestBullet(Vector3 playerPos, out BulletBase closest)
    {
        closest = null;
        float bestSqr = float.PositiveInfinity;

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
