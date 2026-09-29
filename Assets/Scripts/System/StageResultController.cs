using UnityEngine;

/// <summary>
///     ゴール・死亡・時間切れの通知を受け、リザルトシーンへの遷移を管理するクラス
/// </summary>
public class StageResultController : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;
    [SerializeField] private StageCountDownTimer _timer;
    [SerializeField] private StageGoal _goal;
    [SerializeField] private TimeDilationController _timeDilation;
    [SerializeField] private string _resultSceneName = "Result";

    private GameRunOutcome _pendingOutcome;
    private bool _isTransitioning;

    private void OnEnable()
    {
        _playerHealth.OnDied += HandleDeath;
        _timer.OnTimeUp += HandleTimeUp;
        if (_goal != null) _goal.OnReached += HandleGoalReached;
    }

    private void Start()
    {
        GameRunResult.Clear();
        // 常駐するScoreManagerも、次のプレイでは0点から始める。
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();
    }

    private void OnDisable()
    {
        if (_playerHealth != null) _playerHealth.OnDied -= HandleDeath;
        if (_timer != null) _timer.OnTimeUp -= HandleTimeUp;
        if (_goal != null) _goal.OnReached -= HandleGoalReached;
    }

    private void LateUpdate()
    {
        if (_isTransitioning || _pendingOutcome == GameRunOutcome.None) return;
        _isTransitioning = true;

        // ゴールと死亡・時間切れが重なった場合は、失敗を優先する。
        var outcome = _playerHealth.IsDead ? GameRunOutcome.Failure : _pendingOutcome;
        _timer.StopCountDown();
        if (_timeDilation != null) _timeDilation.Stop();
        GameRunResult.Set(outcome);
        SceneLoader.LoadScene(_resultSceneName);
    }

    /// <summary>
    ///     死亡通知を受け、失敗の結果を保持する
    /// </summary>
    private void HandleDeath()
    {
        _pendingOutcome = GameRunOutcome.Failure;
    }

    /// <summary>
    ///     時間切れ通知を受け、失敗の結果を保持する
    /// </summary>
    private void HandleTimeUp()
    {
        _pendingOutcome = GameRunOutcome.Failure;
    }

    /// <summary>
    ///     対象プレイヤーのゴール通知を受け、未確定の場合に成功の結果を保持する
    /// </summary>
    /// <param name="player"> ゴールを通過したプレイヤー </param>
    private void HandleGoalReached(PlayerHealth player)
    {
        if (player != _playerHealth || player.IsDead || _isTransitioning) return;

        if (_pendingOutcome == GameRunOutcome.None)
            _pendingOutcome = GameRunOutcome.Success;
    }
}
