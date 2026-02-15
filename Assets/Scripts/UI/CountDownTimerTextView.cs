using TMPro;
using UnityEngine;

public class CountDownTimerTextView : MonoBehaviour
{
    [SerializeField, Tooltip("参照するタイマー")] private StageCountDownTimer _countDownTimer;
    [SerializeField, Tooltip("残り時間のテキスト")] private TMP_Text _remainingTimeText;

    private void UpdateRemainingTimeText(float remainingTime)
    {
        int toSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = toSeconds / 60;
        int hours = toSeconds % 60;

        _remainingTimeText.text = $"{minutes:00}:{hours:00}";
    }

    private void Start()
    {
        _countDownTimer.OnRemainingTimeChanged += UpdateRemainingTimeText;
    }
    private void OnDestroy()
    {
        _countDownTimer.OnRemainingTimeChanged -= UpdateRemainingTimeText;
    }
}
