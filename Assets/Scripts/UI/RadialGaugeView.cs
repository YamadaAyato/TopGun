using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     円ゲージの表示を管理するクラス
/// </summary>
public class RadialGaugeView : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _centerIconImage;

    /// <summary>
    ///     0〜1の範囲でゲージを設定する
    /// </summary>
    /// <param name="value"></param>
    public void SetNormalized(float value)
    {
        _fillImage.fillAmount = Mathf.Clamp01(value);
    }

    /// <summary>
    ///     アイコンを設定する
    /// </summary>
    /// <param name="icon"></param>
    public void SetCenterIcon(Sprite icon)
    {
        _centerIconImage.sprite = icon;
    }
}
