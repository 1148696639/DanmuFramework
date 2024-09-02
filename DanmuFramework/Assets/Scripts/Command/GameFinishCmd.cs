using QFramework;

/// <summary>
///   游戏结束时调用
/// </summary>
public class GameFinishCmd : AbstractCommand
{
    protected override void OnExecute()
    {
        this.SendEvent<GameFinishEvent>();
    }
}