using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
///     スポーン地点からスコアポップアップを生成するクラス
/// </summary>
public class ScorePouupSpawer : MonoBehaviour
{
    [SerializeField] private GameObject _popupPrefab;
    [SerializeField] private RectTransform _spawnAnchor;

    [SerializeField] private float _moveY;
    [SerializeField] private float _duration;

    /// <summary>
    ///     追加するスコアをポップアップで表示する
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="amount"></param>
    public void SpawnPopup(string reason,int amount)
    {
        // ポップアップ生成
        GameObject popup = Instantiate(_popupPrefab, _spawnAnchor);
        RectTransform rect = popup.GetComponent<RectTransform>();
        TMP_Text text = popup.GetComponent<TMP_Text>();

        Debug.Log($"スコアポップアップ生成: {reason} + {amount}");

        text.text = $" {reason} + {amount}";
        text.alpha = 0f;

        // アニメーション開始
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, _moveY);

        rect.anchoredPosition = startPos;

        Sequence seq = DOTween.Sequence();

        // フェードインと移動、フェードアウト
        seq.Append(text.DOFade(1f, 0.2f));
        seq.Join(rect.DOAnchorPos(endPos, _duration).SetEase(Ease.OutCubic));
        seq.Append(text.DOFade(0f, _duration))
           .SetUpdate(true);

        // 終了時にポップアップを破棄
        seq.OnComplete(() => Destroy(popup));
    }

    private void OnEnable()
    {
        ScoreManager.Instance.OnScoreAdded += SpawnPopup;
    }

    private void OnDisable()
    {
        ScoreManager.Instance.OnScoreAdded -= SpawnPopup;
    }
}
