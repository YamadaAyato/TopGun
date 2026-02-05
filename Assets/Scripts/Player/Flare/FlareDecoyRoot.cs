using UnityEngine;


public class FlareDecoyRoot : MonoBehaviour, IFlareKillable
{
    [Header("フレア設定")]
    [SerializeField] private float _lifeTime = 5f;
    [SerializeField] private float _blastRadius;

    [Header("各レイヤー設定")]
    [SerializeField] private LayerMask _bulletLayer;
    [SerializeField] private LayerMask _flareLayer;

    private float _timer;
    private bool _detonated;

    public void KillByFlare(Vector3 hitPoint)
    {
        if (_detonated) return;
        _detonated = true;
        Destroy(this.gameObject);
    }

    public void Detonate(Vector3 hitPoint, IKillableBullet bullet)
    {
        if (_detonated) return;
        _detonated = true;

        Debug.Log("Flare Decoy Detonated");
        bullet?.Kill(hitPoint);

        var bullets = Physics.OverlapSphere
            (transform.position,
            _blastRadius,
            _bulletLayer,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < bullets.Length; i++)
        {
            if (bullets[i].TryGetComponent<IKillableBullet>(out var b))
            {
                b.Kill(transform.position);
            }
        }

        var flareBullets = Physics.OverlapSphere
            (transform.position,
            _blastRadius,
            _flareLayer,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < flareBullets.Length; i++)
        {
            if (flareBullets[i].TryGetComponent<IFlareKillable>(out var f))
            {
                f.KillByFlare(transform.position);
            }
        }

        Destroy(this.gameObject);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _blastRadius);
    }
}
