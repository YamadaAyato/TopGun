using UnityEngine;

/// <summary>
///     弾を検知してデコイターゲットを設定するコライダー
/// </summary>
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "BulletDetectingFlareColider")]
public class BulletDetectingFlareCollider : MonoBehaviour
{
    private FlareDecoyRoot _flareDecoyRoot;

    private void Awake()
    {
        _flareDecoyRoot = GetComponentInParent<FlareDecoyRoot>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDecoyAttractable>(out IDecoyAttractable attractable))
        {
            attractable.SetDecoyTarget(_flareDecoyRoot.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IDecoyAttractable>(out IDecoyAttractable attractable))
        {
            attractable.ClearDecoyTarget(_flareDecoyRoot.transform);
        }
    }
}
