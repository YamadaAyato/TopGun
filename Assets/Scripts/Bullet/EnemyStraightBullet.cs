using Unity.AppUI.Core;
using UnityEngine;

/// <summary>
///     敵の銃弾処理をするクラス
/// </summary>
public class EnemyStraightBullet : BulletBase
{
    private Vector3 _dir = Vector3.forward;

    /// <summary>
    ///     方向を設定する
    /// </summary>
    /// <param name="dir"></param>
    public void SetDirection(Vector3 dir)
    {
        _dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : transform.forward;
        transform.rotation = Quaternion.LookRotation(_dir, Vector3.up);
    }

    protected override void HandleHit(Collider other)
    {
        if (other.TryGetComponent<PlayerHealth>(out PlayerHealth hit))
        {
            if (hit.CanBeHit)
            {
                hit.TakeDamage(_damage);
                AudioManager.Instance.PlaySE3D("PlayerDamage", this.transform);
            }

            Release();
            return;
        }

        if (other.CompareTag("Obstacle"))
        {
            Release();
        }
    }

    protected override void Update()
    {
        transform.position += _dir * _bulletSpeed * Time.deltaTime;

        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            Release();
        }
    }
}
