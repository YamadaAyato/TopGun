using UnityEngine;

/// <summary>
///     フレアの本体コライダー
/// </summary>
public class FlareCore : MonoBehaviour
{
    private FlareDecoyRoot _flareDecoyRoot;

    private void Awake()
    {
        _flareDecoyRoot = GetComponentInParent<FlareDecoyRoot>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IKillableBullet>(out IKillableBullet bullet))
        {
            _flareDecoyRoot.Detonate(transform.position, bullet);
        }
    }
}
