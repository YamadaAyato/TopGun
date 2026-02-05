using UnityEngine;

/// <summary>
///     外部から消されることができる弾のインターフェース
/// </summary>
public interface IKillableBullet
{
    /// <summary> 弾を消す </summary>
    /// <param name="hitPoint"> ヒット時などエフェクトを発生させる用のポジション </param>
    void Kill(Vector3 hitPoint);
}
