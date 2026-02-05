using UnityEngine;

public interface IDecoyAttractable
{
    void SetDecoyTarget(Transform decoyTransform);
    void ClearDecoyTarget(Transform decoyTransform);
}
