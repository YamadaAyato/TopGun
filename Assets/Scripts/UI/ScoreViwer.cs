using TMPro;
using UnityEngine;

public class ScoreViwer : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;

    private void UpdateText(int score)
    {
        _scoreText.text = $"Score: {score}";
    }

    private void OnEnable()
    {
        ScoreManager.Instance.OnScoreChanged += UpdateText;
    }

    private void OnDisable()
    {
        ScoreManager.Instance.OnScoreChanged -= UpdateText;
    }
}
