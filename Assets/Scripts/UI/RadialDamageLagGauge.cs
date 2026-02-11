using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RadialDamageLagGauge : MonoBehaviour
{
    [SerializeField] private Image _mainFillImage;
    [SerializeField] private Image _lagFillImage;

    [SerializeField] private float _lagDelay;
    [SerializeField] private float _catchUpSpeed;

    private Tween _damageTween;

    public void SetNormalized(float value)
    {
        // メインゲージを即座に更新
        _mainFillImage.fillAmount = Mathf.Clamp01(value);
        // ラグゲージの更新をキャンセル
        _damageTween?.Kill();
        // ラグゲージを遅延後に追従させる
        float startValue = _lagFillImage.fillAmount;
        float endValue = Mathf.Clamp01(value);
        _damageTween = DOTween.To(
            () => _lagFillImage.fillAmount,
            x => _lagFillImage.fillAmount = x,
            endValue,
            _catchUpSpeed
            )
            .SetDelay(_lagDelay)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }


    private void OnDisable()
    {
        _damageTween?.Kill();
        _damageTween = null;
    }
}
