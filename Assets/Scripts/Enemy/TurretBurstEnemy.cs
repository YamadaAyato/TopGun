using System.Collections;
using UnityEngine;

/// <summary>
///     連射するタレット型の敵クラス
/// </summary>
public class TurretBurstEnemy : TurretEnemyBase
{
    [Header("連射タレット設定")]
    [SerializeField, Tooltip("連射数")] private int _burstCount;
    [SerializeField, Tooltip("連射間隔")] private float _burstInterval;
    [SerializeField, Tooltip("弾のばらつき")] private float _spreadAngle;

    private Coroutine _burstCoroutine;

    protected override void OnSpawned()
    {
        base.OnSpawned();
    }

    protected override void FireAtPlayer(Transform player)
    {
        if (_burstCoroutine == null)
        {
            _burstCoroutine = StartCoroutine(BurstFireCoroutine(player));
        }
    }

    /// <summary>
    ///     連射を行うコルーチン
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator BurstFireCoroutine(Transform player)
    {
        // 連射回数分発射
        for (int i = 0; i < _burstCount; i++)
        {
            ShootOnce(player);
            // 連射間隔待機
            yield return new WaitForSeconds(_burstInterval);
        }
        _burstCoroutine = null;
    }

    /// <summary>
    ///     一つの弾を発射する
    /// </summary>
    /// <param name="player"></param>
    private void ShootOnce(Transform player)
    {
        Vector3 toPlayer = (player.position - _muzzle.position).normalized;

        if (_spreadAngle > 0f)
        {
            // ばらつきを加える
            float randomY = Random.Range(-_spreadAngle, _spreadAngle);
            float randomX = Random.Range(-_spreadAngle, _spreadAngle);
            Quaternion spreadRotation = Quaternion.Euler(randomX, randomY, 0f);
            toPlayer = (spreadRotation * toPlayer).normalized;
        }

        _shooter.FireBurst(this.transform, toPlayer);
    }
}
