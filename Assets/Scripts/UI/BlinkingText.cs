using DG.Tweening;
using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _blinkInterval;

    private Tween _tween;

    private void StartBlink()
    {
        _tween = _text.DOFade(0.15f, _blinkInterval)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void Start()
    {
        StartBlink();
    }
}
