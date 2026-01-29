using UnityEngine;

public class PlayerCounterBullet : BulletBase
{
    [Header("ホーミング設定")]
    [SerializeField] private float _turnSpeed;
    [SerializeField] private float _hormingDuration;
    [SerializeField] private float _maxSeekAngle;

    private Transform _target;
    private float _hormingTimer;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    protected override void OnSpawned()
    {
        _hormingTimer = 0f;
    }
    protected override void HandleHit(Collider other)
    {
        if(other.TryGetComponent<EnemyBase>(out EnemyBase hit))
        {
            hit.TakeDamage(_damage);
            Release();
            return;
        }

        if(other.CompareTag("Obstacle"))
        {
            Release();
        }
    }

    protected override void Update()
    {
        // ホーミング（向きを曲げる）
        if (_hormingTimer < _hormingDuration && _target != null)
        {
            Vector3 toTarget = (_target.position - transform.position);
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;

                float angleToTarget = Vector3.Angle(transform.forward, dir);
                if (angleToTarget <= _maxSeekAngle)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        _turnSpeed * Time.deltaTime
                    );
                }
            }

            _hormingTimer += Time.deltaTime;
        }

        // 移動＋寿命はベースに任せる
        base.Update();
    }
}
