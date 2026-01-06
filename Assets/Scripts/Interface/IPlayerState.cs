/// <summary>
///     プレイヤーの状態を表すインターフェース
///     PlayerStateMachineから使用される
/// </summary>
public interface IPlayerState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}
