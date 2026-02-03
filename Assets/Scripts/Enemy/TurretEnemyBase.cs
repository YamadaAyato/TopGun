using UnityEngine;

public abstract class TurretEnemyBase : EnemyBase
{
    [SerializeField] protected Transform _muzzle;
    [SerializeField] protected float _range;
    [SerializeField, Range(0f, 180f)] protected float _fovAngle;
    [SerializeField] protected float _fireInterval;
    [SerializeField] protected LayerMask _layerMask;

    protected float _timer;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        _timer = 0;
    }

    protected abstract void FireAtPlayer(Transform player);

    protected bool TryGetPlayer(out Transform player)
    {
        player = PlayerLocator.Instance.PlayerTransform;
        return player != null;
    }

    protected bool IsPlayerInRange(Transform player)
    {
        float distance = Vector3.Distance(this.transform.position, player.position);
        return distance <= _range;
    }

    protected bool IsPlayerInFov(Transform player)
    {
        Vector3 toPlayer = (player.position - this.transform.position).normalized;
        float angle = Vector3.Angle(this.transform.forward, toPlayer);
        return angle <= _fovAngle * 0.5f;
    }

    protected bool HasLineOfSight(Transform player)
    {
        Vector3 direction = (player.position - _muzzle.position).normalized;
        float distance = Vector3.Distance(_muzzle.position, player.position);
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
        if(_timer > _fireInterval)
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
