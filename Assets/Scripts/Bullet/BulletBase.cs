using System;
using UnityEngine;

/// <summary>
///     銃弾の基底クラス
/// </summary>
public abstract class BulletBase : MonoBehaviour
{
    public Transform Shooter => _shooter;

    [Header("弾設定")]
    [SerializeField] protected float _bulletSpeed;
    [SerializeField] protected float _lifeTime;

    protected Transform _shooter;
    protected float _timer;
    private Action<BulletBase> _onRelease;

    public void Spawn(Action<BulletBase> onRelease,Transform parent)
    {
        _onRelease = onRelease;
        _shooter = parent;
        _timer = 0;
        OnSpawned();
    }

    protected void OnSpawned() { }
    
    /// <summary> 銃弾HIt時の処理をする。 </summary>
    protected abstract void HandleHit(Collider other);

    /// <summary> 銃弾消滅処理 </summary>
    protected void Release()
    {
        _onRelease?.Invoke(this);

        _onRelease = null;
        _shooter = null;
        _timer = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    protected virtual void Update()
    {
        this.transform.position += transform.forward * _bulletSpeed * Time.deltaTime;

        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            Release();
        }
    }
}
