using UnityEngine;

/// <summary>
///     フレアによって倒されることができるオブジェクトのインターフェース
/// </summary>
public interface IFlareKillable
{
    /// <summary> フレアによって倒される </summary>
    void KillByFlare(Vector3 hitPoint);
}
