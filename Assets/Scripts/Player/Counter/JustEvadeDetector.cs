using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     ジャスト回避用の近くの銃弾を探知し、保持するクラス
/// </summary>
public class JustEvadeDetector : MonoBehaviour
{
    [SerializeField] private float _justDistance;

    private readonly HashSet<Transform> _bullets = new();

    /// <summary>
    ///     現在ジャスト距離に弾があるか
    /// </summary>
    /// <param name="playerPos"></param>
    /// <returns></returns>
    public bool IsJustEvade(Vector3 playerPos)
    {
        foreach (var bullet in _bullets)
        {
            if (Vector3.Distance(bullet.position, playerPos) <= _justDistance)
            {
                return true;
            }
        }
        return false;
    }

    // ======================追加と削除======================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            _bullets.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            _bullets.Remove(other.transform);
        }
    }
}
