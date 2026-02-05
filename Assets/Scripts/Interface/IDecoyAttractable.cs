using UnityEngine;

/// <summary>
///     デコイによって引き寄せられることができるオブジェクトのインターフェース
/// </summary>
public interface IDecoyAttractable
{
    /// <summary> ターゲットをデコイに設定する </summary>
    /// <param name="decoyTransform"></param>
    void SetDecoyTarget(Transform decoyTransform);
    /// <summary> ターゲットをデコイから外す </summary>
    /// <param name="decoyTransform"></param>
    void ClearDecoyTarget(Transform decoyTransform);
}
