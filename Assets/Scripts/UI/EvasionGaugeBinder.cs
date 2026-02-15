using TMPro;
using UnityEngine;

/// <summary>
///     回避ゲージとUIを紐づけるクラス
/// </summary>
public class EvasionGaugeBinder : MonoBehaviour
{
    [SerializeField] private EvationGauge _evasionGauge;
    [SerializeField] private RadialGaugeView _gaugeView;
    [SerializeField] private ThresholdLinesView _thresholdLinesView;

    [SerializeField] private TMP_Text _usableCountText;

    private void OnEnable()
    {
        _evasionGauge.OnChargesChanged += HandleChanged;
        _thresholdLinesView.Setup(_evasionGauge.MaxCharges);
    }

    private void Start()
    {
        // 初期化用
        HandleChanged(_evasionGauge.CurrentCharges, _evasionGauge.MaxCharges);
    }

    private void OnDisable()
    {
        _evasionGauge.OnChargesChanged -= HandleChanged;
    }

    /// <summary>
    ///     ゲージが変化したときの処理
    /// </summary>
    /// <param name="currentCharges"></param>
    /// <param name="maxCharges"></param>
    private void HandleChanged(float currentCharges, int maxCharges)
    {
        // 正規化してゲージにセット
        float normalized = Mathf.Clamp01(currentCharges / maxCharges);
        _gaugeView.SetNormalized(normalized);

        // 使える回数をセット
        int usableCount = Mathf.FloorToInt(currentCharges);
        _usableCountText.text = $"{usableCount} / {maxCharges}";
    }
}
