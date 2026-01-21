using UnityEngine;

public abstract class BulletBase : MonoBehaviour
{
    [Header("弾設定")]
    [SerializeField] protected float _bulletSpeed;
    [SerializeField] protected float _lifeTime;

    protected float _timer;

    protected virtual void Update()
    {
        this.transform.position += transform.forward * _bulletSpeed * Time.deltaTime;
    }

    protected abstract void HandleHit(Collider other);

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

}
