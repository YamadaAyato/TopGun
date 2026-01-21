using UnityEngine;

public class TurretEnemy : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private EnemyBullet _bulletPrefab;

    [SerializeField] private float _fireInterval;
    [SerializeField] private int _poolSize;

    private ObjectPool<EnemyBullet> _bulletPool;
    private float _timer;

    private void Update()
    {

    }
}
