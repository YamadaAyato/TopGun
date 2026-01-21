using UnityEngine;

/// <summary>
///     銃弾の基底クラス
/// </summary>
public abstract class BulletBase : MonoBehaviour
{
    [Header("弾設定")]
    [SerializeField] protected float _bulletSpeed;
    [SerializeField] protected float _lifeTime;

    protected float _timer;

    protected virtual void Update()
    {
        this.transform.position += transform.forward * _bulletSpeed * Time.deltaTime;

        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            Release();
        }
    }
    
    /// <summary> 銃弾HIt時の処理をする。 </summary>
    protected abstract void HandleHit(Collider other);

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    /// <summary> 銃弾消滅処理 </summary>
    protected void Release()
    {
        Destroy(this.gameObject);
    }
}
