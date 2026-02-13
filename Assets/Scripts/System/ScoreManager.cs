using System;
using UnityEngine;

/// <summary>
///     スコアの管理を行うシングルトンクラス
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    /// <summary> 現在のスコア </summary>
    public int CurrentScore => _score;

    /// <summary> スコアが変化したときのイベント </summary>
    public event Action<int> OnScoreChanged;

    /// <summary> スコアが追加されたときのイベント </summary>
    public event Action<string,int> OnScoreAdded;

    private int _score;

    /// <summary>
    ///     スコアを追加する
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="reason"></param>
    public void AddScore(int amount,string reason)
    {
        _score += amount;

        OnScoreAdded?.Invoke(reason, amount);
        OnScoreChanged?.Invoke(_score);
    }

    /// <summary>
    ///     スコアをリセットする
    /// </summary>
    public void ResetScore()
    {
        _score = 0;
        OnScoreChanged?.Invoke(_score);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        ResetScore();
    }

    private void OnEnable()
    {
        GameEvents.OnScoreAdd += AddScore;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreAdd -= AddScore;
    }
}
