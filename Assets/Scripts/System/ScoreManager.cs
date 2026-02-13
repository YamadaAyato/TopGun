using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score => _score;

    public event Action<int> OnScoreChanged;

    public event Action<string,int> OnScoreAdded;

    private int _score;

    public void AddScore(int amount,string reason)
    {
        _score += amount;

        OnScoreAdded?.Invoke(reason, amount);
        OnScoreChanged?.Invoke(_score);
    }

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
