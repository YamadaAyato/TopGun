using UnityEngine;

/// <summary>
///     タレット敵の基底クラス
/// </summary>
public abstract class TurretEnemyBase : EnemyBase
{
    [Header("タレット共通設定")]
    [SerializeField, Tooltip("銃口")] protected Transform _muzzle;
    [SerializeField, Tooltip("プレイヤーを見つける距離")] protected float _range;
    [SerializeField, Range(0f, 180f), Tooltip("視野角")] protected float _fovAngle;
    [SerializeField, Tooltip("撃つ間隔")] protected float _fireInterval;
    [SerializeField, Tooltip("判定レイヤー")] protected LayerMask _layerMask;

    protected float _timer;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        _timer = 0;
    }

    /// <summary>
    ///     プレイヤーに向けて撃つ
    /// </summary>
    /// <param name="player"> プレイヤーの位置 </param>
    protected abstract void FireAtPlayer(Transform player);

    /// <summary>
    ///     プレイヤーのTransformを取得する
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected bool TryGetPlayer(out Transform player)
    {
        player = PlayerLocator.Instance.PlayerTransform;
        return player != null;
    }

    /// <summary>
    ///     射程内にプレイヤーがいるか
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected bool IsPlayerInRange(Transform player)
    {
        float distance = Vector3.Distance(_muzzle.position, player.position);
        return distance <= _range;
    }

    /// <summary>
    ///     プレイヤーが視野内にいるか
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected bool IsPlayerInFov(Transform player)
    {
        Vector3 toPlayer = (player.position - _muzzle.position).normalized;
        // タレットの前方とプレイヤーへの方向との角度を計算
        float angle = Vector3.Angle(this.transform.forward, toPlayer);
        return angle <= _fovAngle * 0.5f;
    }

    /// <summary>
    ///     遮蔽物がなくプレイヤーが見えているか
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected bool HasLineOfSight(Transform player)
    {
        Vector3 direction = (player.position - _muzzle.position).normalized;
        float distance = Vector3.Distance(_muzzle.position, player.position);

        // レイキャストで遮蔽物がないか確認
        if (Physics.Raycast(_muzzle.position, direction, out RaycastHit hit, distance, _layerMask))
        {
            if (hit.transform != player)
            {
                return false;
            }
        }
        return true;
    }

    protected virtual void Update()
    {
        _timer += Time.deltaTime;

        // 一定時間ごとにプレイヤーを探して撃つ
        if (_timer > _fireInterval)
        {
            if (TryGetPlayer(out Transform player) &&
                IsPlayerInRange(player) &&
                IsPlayerInFov(player) &&
                HasLineOfSight(player))
            {
                FireAtPlayer(player);
                _timer = 0f;
            }
        }
    }
}
