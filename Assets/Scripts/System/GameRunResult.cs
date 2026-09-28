using UnityEngine;

/// <summary>
///     ゲームの終了結果
/// </summary>
public enum GameRunOutcome
{
    None,
    Success,
    Failure
}

/// <summary>
///     ゲームシーンからリザルトシーンへ渡す結果を保持するクラス
/// </summary>
public static class GameRunResult
{
    /// <summary> 現在のゲーム結果 </summary>
    public static GameRunOutcome Outcome { get; private set; }

    /// <summary>
    ///     ゲームの結果を未設定の状態に戻す
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        Outcome = GameRunOutcome.None;
    }

    /// <summary>
    ///     リザルトシーンに渡すゲームの結果を設定する
    /// </summary>
    /// <param name="outcome"> 終了したゲームの結果 </param>
    public static void Set(GameRunOutcome outcome)
    {
        Outcome = outcome;
    }
}
