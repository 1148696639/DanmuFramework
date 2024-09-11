using QFramework;

/// <summary>
///     主播点击开始游戏之后调用，进入准备阶段（期间可以动态加载资源，切换场景等），准备完成之后调用GameStartCmd
/// </summary>
public class GamePrepareCmd : AbstractCommand
{
    protected override void OnExecute()
    {
        DebugCtrl.Log("游戏进入准备阶段...");

        this.SendEvent<GamePrepareEvent>();
    }
}