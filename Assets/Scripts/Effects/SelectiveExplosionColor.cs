using UnityEngine;

/// <summary>
///     色を残す爆発のレイヤーを一時的に切り替えるクラス
/// </summary>
public class SelectiveExplosionColor : MonoBehaviour
{
    private Transform[] _parts;
    private int[] _layers;

    /// <summary>
    ///     爆発を専用の描画レイヤーへ移す
    /// </summary>
    public void Apply(int layer)
    {
        Restore();
        _parts = GetComponentsInChildren<Transform>(true);
        _layers = new int[_parts.Length];
        for (int i = 0; i < _parts.Length; i++)
        {
            _layers[i] = _parts[i].gameObject.layer;
            _parts[i].gameObject.layer = layer;
        }
    }

    /// <summary>
    ///     プールへ戻す前に元のレイヤーへ戻す
    /// </summary>
    public void Restore()
    {
        if (_parts == null) return;
        for (int i = 0; i < _parts.Length; i++)
            if (_parts[i] != null) _parts[i].gameObject.layer = _layers[i];
        _parts = null;
        _layers = null;
    }

    private void OnDisable() => Restore();
}
