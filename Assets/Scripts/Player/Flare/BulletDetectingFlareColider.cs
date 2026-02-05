using UnityEngine;

public class BulletDetectingFlareColider : MonoBehaviour
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
