using UnityEngine;

/// <summary>
///     閾値を示す線分を管理するクラス
/// </summary>
public class ThresholdLinesView : MonoBehaviour
{
    [SerializeField] private RectTransform _line1;
    [SerializeField] private RectTransform _line2;
    [SerializeField] private RectTransform _line3;

    [SerializeField, Tooltip("Filledの開始角に合わせる補正")] private float _startAngleDeg;

    /// <summary>
    ///     Lineをセットアップする
    /// </summary>
    /// <param name="maxCharges"></param>
    public void Setup(int maxCharges)
    {
        SetLine(_line1, 1f / maxCharges);
        SetLine(_line2, 2f / maxCharges);
        SetLine(_line3, 3f / maxCharges);
    }

    private void SetLine(RectTransform line, float t)
    {
        float angle = _startAngleDeg - (t * 360f);
        line.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
