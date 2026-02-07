using UnityEngine;

public enum EnemyShooterType
{
    Straight,
    Homing,
    Random
}

public class EnemyShooter : MonoBehaviour, IEnemyShooter
{


    public void Fire(Transform shooter, Transform target)
    {
        throw new System.NotImplementedException();
    }
}
