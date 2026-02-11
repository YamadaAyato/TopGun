using UnityEngine;
using UnityEngine.UI;

public class RadialGaugeView : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private Image _centerIconImage;

    public void SetNormalized(float value)
    {
        _fillImage.fillAmount = Mathf.Clamp01(value);
    }

    public void SetCenterIcon(Sprite icon)
    {
        _centerIconImage.sprite = icon;
    }
}
