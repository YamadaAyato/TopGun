using TMPro;
using UnityEngine;

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
        HandleChanged(_evasionGauge.CurrentCharges, _evasionGauge.MaxCharges);
    }

    private void OnDisable()
    {
        _evasionGauge.OnChargesChanged -= HandleChanged;
    }

    private void HandleChanged(float currentCharges, int maxCharges)
    {
        float normalized = Mathf.Clamp01(currentCharges / maxCharges);
        _gaugeView.SetNormalized(normalized);

        int usableCount = Mathf.FloorToInt(currentCharges);
        _usableCountText.text = usableCount.ToString();
    }
}
