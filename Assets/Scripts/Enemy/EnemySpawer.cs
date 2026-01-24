using UnityEngine;

public class EnemySpawer : MonoBehaviour
{
    [SerializeField] TurretEnemy _turretPrefab;
    [SerializeField] Transform _exsampleTransform;
    [SerializeField] private int _poolSize;

    private ObjectPool<TurretEnemy> _pool;

    public TurretEnemy SpawnTurret(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        TurretEnemy turret = _pool.Get();
        turret.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        turret.Spawn(ReturnEnemy);
        return turret;
    }

    private void ReturnEnemy(EnemyBase enemy)
    {
        _pool.Release((TurretEnemy)enemy);
    }

    private void Awake()
    {
        _pool = new ObjectPool<TurretEnemy>(_turretPrefab, _exsampleTransform, _poolSize);
    }
}
