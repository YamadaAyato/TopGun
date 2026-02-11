using UnityEngine;

/// <summary>
///     フレアデコイの根本クラス
/// </summary>
public class FlareDecoyRoot : MonoBehaviour, IFlareKillable
{
    [Header("フレア設定")]
    [SerializeField] private float _lifeTime;
    [SerializeField] private float _blastRadius;

    [Header("各レイヤー設定")]
    [SerializeField] private LayerMask _bulletLayer;
    [SerializeField] private LayerMask _flareLayer;

    private float _timer;
    private bool _detonated;

    /// <summary>
    ///     フレアを撃ち落とした時の処理
    /// </summary>
    /// <param name="hitPoint"></param>
    public void KillByFlare(Vector3 hitPoint)
    {
        if (_detonated) return;
        _detonated = true;
        GameEvents.RaiseExplosion(transform.position);
        Destroy(this.gameObject);
    }

    /// <summary>
    ///     消去処理
    /// </summary>
    /// <param name="hitPoint"></param>
    /// <param name="bullet"></param>
    public void Detonate(Vector3 hitPoint, IKillableBullet bullet)
    {
        if (_detonated) return;
        _detonated = true;

        Debug.Log("Flare Decoy Detonated");
        // 最初に当たった弾を消す
        bullet?.Kill(hitPoint);

        // 爆発範囲内の弾を消す
        var bullets = Physics.OverlapSphere
            (transform.position,
            _blastRadius,
            _bulletLayer,
            QueryTriggerInteraction.Collide);

        // 消去処理
        for (int i = 0; i < bullets.Length; i++)
        {
            if (bullets[i].TryGetComponent<IKillableBullet>(out var b))
            {
                b.Kill(transform.position);
            }
        }

        // 範囲内の他のフレアデコイを消す
        var flareBullets = Physics.OverlapSphere
            (transform.position,
            _blastRadius,
            _flareLayer,
            QueryTriggerInteraction.Collide);

        // 消去処理
        for (int i = 0; i < flareBullets.Length; i++)
        {
            if (flareBullets[i].TryGetComponent<IFlareKillable>(out var f))
            {
                f.KillByFlare(transform.position);
            }
        }

        GameEvents.RaiseExplosion(transform.position);
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
