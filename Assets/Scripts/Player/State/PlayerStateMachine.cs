using UnityEngine;

/// <summary>　
///     プレイヤーの状態を管理するクラス　
/// </summary>
public class PlayerStateMachine : MonoBehaviour
{
    private IPlayerState _currentState;

    /// <summary>　状態を変更する　</summary>
    /// <param name="newState"></param>
    public void ChangeState(IPlayerState newState)
    {
        _currentState?.OnExit();
        _currentState = newState;
        _currentState?.OnEnter();
    }

    private void Update()
    {
        _currentState?.OnUpdate();
    }
}
