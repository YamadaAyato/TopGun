using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCounterBullet : BulletBase
{
    [Header("ホーミング設定")]
    [SerializeField] private float _turnSpeed;
    [SerializeField] private float _homingDuration;
    [SerializeField] private float _maxSeekAngle;

    [Header("挙動補助")]
    [SerializeField] private float _initialForwardTime;
    [SerializeField] private float _hitRadius;
    [SerializeField] private float _arriveSlowdownRadius;
    [SerializeField] private float _minSpeedFactorNear;

    private Transform _target;
    private float _homingTimer;
    private float _forwardTimer;
    private Rigidbody _rb;
    private Quaternion _launchRotation;
    private Vector3 _launchForward;

    public void SetTarget(Transform target, Vector3 shooterForward)
    {
        _target = target;
        _launchForward = shooterForward.sqrMagnitude > 0.0001f ? shooterForward.normalized : transform.forward;
    }

    protected override void OnSpawned()
    {
        _homingTimer = 0f;
        _forwardTimer = 0f;

        if (_rb == null) _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;

        _launchRotation = _rb.rotation;

        if (_launchForward.sqrMagnitude < 0.0001f)
            _launchForward = transform.forward;
    }

    protected override void HandleHit(Collider other)
    {
        if (other.transform == Shooter) return;

        if (other.TryGetComponent<EnemyBase>(out EnemyBase hit))
        {
            hit.TakeDamage(_damage);
            Release();
            return;
        }

        if (other.CompareTag("Obstacle"))
        {
            Release();
        }
    }

    private void FixedUpdate()
    {
        _timer += Time.fixedDeltaTime;
        if (_timer >= _lifeTime)
        {
            Release();
            return;
        }

        if (_target != null && _hitRadius > 0f)
        {
            float dist = Vector3.Distance(_rb.position, _target.position);
            if (dist <= _hitRadius)
            {
                if (_target.TryGetComponent<EnemyBase>(out var enemy))
                {
                    enemy.TakeDamage(_damage);
                }
                Release();
                return;
            }
        }

        bool canHome = _target != null;
        if (_homingDuration > 0f)
        {
            _homingTimer += Time.fixedDeltaTime;
            if (_homingTimer > _homingDuration) canHome = false;
        }

        _forwardTimer += Time.fixedDeltaTime;
        bool isForwardPhase = (_forwardTimer < _initialForwardTime);

        Quaternion nextRot = _rb.rotation;

        if (isForwardPhase)
        {
            nextRot = _launchRotation;
        }
        else if (canHome)
        {
            Vector3 toTarget = _target.position - _rb.position;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;

                float angle = Vector3.Angle(transform.forward, dir);
                if (angle <= _maxSeekAngle)
                {
                    Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
                    float maxStep = _turnSpeed * Time.fixedDeltaTime;
                    nextRot = Quaternion.RotateTowards(_rb.rotation, desired, maxStep);
                }
            }
        }

        _rb.MoveRotation(nextRot);

        float speedNow = _bulletSpeed;

        if (_target != null && _arriveSlowdownRadius > 0f)
        {
            float dist = Vector3.Distance(_rb.position, _target.position);
            float k = Mathf.Clamp01(dist / _arriveSlowdownRadius);
            float factor = Mathf.Lerp(_minSpeedFactorNear, 1f, k);
            speedNow *= factor;
        }

        Vector3 moveDir = isForwardPhase ? _launchForward : (nextRot * Vector3.forward);
        Vector3 nextPos = _rb.position + moveDir * speedNow * Time.fixedDeltaTime;
        _rb.MovePosition(nextPos);
    }

    protected override void Update() { }
}
