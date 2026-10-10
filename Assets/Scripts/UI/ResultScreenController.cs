using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     リザルトの表示とタイトルシーンへの遷移を管理するクラス
/// </summary>
public class ResultScreenController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private TMP_Text _outcomeText;
    [SerializeField] private UnityEngine.UI.Button _titleButton;

    [Header("シーン設定")]
    [SerializeField, Tooltip("タイトルへ戻る際のシーン名")] private string _titleSceneName = "Title";

    [Header("表示設定")]
    [SerializeField, Tooltip("成功時に表示する文字")] private string _successText = "成功";
    [SerializeField, Tooltip("失敗時に表示する文字")] private string _failureText = "失敗";
    [SerializeField, Tooltip("結果が未設定の場合に表示する文字")] private string _defaultText = "リザルト";
    [SerializeField, Tooltip("成功時の文字色")] private Color _successColor = new Color(0.35f, 0.9f, 0.75f);
    [SerializeField, Tooltip("失敗時の文字色")] private Color _failureColor = new Color(1f, 0.4f, 0.4f);

    private bool _isLeaving;

    /// <summary>
    ///     ボタンの連打を防止し、タイトルシーンへ遷移する
    /// </summary>
    public void ReturnToTitle()
    {
        if (_isLeaving) return;
        _isLeaving = true;
        _titleButton.interactable = false;
        SceneLoader.LoadScene(_titleSceneName);
    }

    private void OnEnable()
    {
        _titleButton.onClick.AddListener(ReturnToTitle);
    }

    private void Start()
    {
        UpdateOutcomeText();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_titleButton.gameObject);
    }

    private void OnDisable()
    {
        _titleButton.onClick.RemoveListener(ReturnToTitle);
    }

    /// <summary>
    ///     ゲームの結果に合わせて表示文字と色を更新する
    /// </summary>
    private void UpdateOutcomeText()
    {
        switch (GameRunResult.Outcome)
        {
            case GameRunOutcome.Success:
                _outcomeText.text = _successText;
                _outcomeText.color = _successColor;
                break;
            case GameRunOutcome.Failure:
                _outcomeText.text = _failureText;
                _outcomeText.color = _failureColor;
                break;
            default:
                _outcomeText.text = _defaultText;
                break;
        }
    }
}
