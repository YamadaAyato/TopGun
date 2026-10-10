using UnityEngine;

/// <summary>
///     プールから砲台を生成し、返却を管理する。
/// </summary>
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "EnemySpawer")]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private TurretEnemy _turretPrefab;
    [SerializeField] private Transform _exsampleTransform;
    [SerializeField] private int _poolSize;

    private ObjectPool<TurretEnemy> _pool;

    public TurretEnemy SpawnTurret(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        TurretEnemy turret = _pool.Get();
        turret.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        turret.Spawn(ReturnEnemy);
        return turret;
    }

    private void Awake()
    {
        _pool = new ObjectPool<TurretEnemy>(_turretPrefab, _exsampleTransform, _poolSize);
    }

    private void ReturnEnemy(EnemyBase enemy)
    {
        _pool.Release((TurretEnemy)enemy);
    }
}
