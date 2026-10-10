using TMPro;
using UnityEngine;

/// <summary>
///     スコアの表示を行うクラス
/// </summary>
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "ScoreViwer")]
public class ScoreViewer : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;

    private void OnEnable()
    {
        ScoreManager.Instance.OnScoreChanged += UpdateText;
    }

    private void OnDisable()
    {
        ScoreManager.Instance.OnScoreChanged -= UpdateText;
    }

    private void UpdateText(int score)
    {
        _scoreText.text = $"Score: {score}";
    }
}
