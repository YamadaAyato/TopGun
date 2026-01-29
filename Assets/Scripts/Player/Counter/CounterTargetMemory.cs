using UnityEngine;

public class CounterTargetMemory : MonoBehaviour
{
    public Transform CurrentTarget => _currentTarget;
 
    private Transform _currentTarget;
    public void SetBullet(BulletBase bullet)
    {
        _currentTarget = bullet.Shooter;
    }

    public void Clear()
    {
        _currentTarget = null;
    }
}
