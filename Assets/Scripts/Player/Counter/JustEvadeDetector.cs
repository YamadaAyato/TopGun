using System.Collections.Generic;
using UnityEngine;

public class JustEvadeDetector : MonoBehaviour
{
    [SerializeField] private float _justDistance;

    private readonly HashSet<Transform> _bullets = new();

    public bool IsJustEvade(Vector3 playerPos)
    {
        foreach (var bullet in _bullets)
        {
            if (Vector3.Distance(bullet.position, playerPos) <= _justDistance)
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            _bullets.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            _bullets.Remove(other.transform);
        }
    }
}
