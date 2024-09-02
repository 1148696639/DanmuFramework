using QFramework;

public class GamePrepareCmd : AbstractCommand
{
    private readonly string _token;

    /// <summary>
    ///  主播点击开始游戏之后调用，进入准备阶段（期间可以动态加载资源，切换场景等），准备完成之后调用GameStartCmd
    /// </summary>
    /// <param name="token"></param>
    public GamePrepareCmd(string token)
    {
        _token = token;
    }

    protected override void OnExecute()
    {
        DebugCtrl.Log("游戏进入准备阶段...");
        this.SendEvent<GamePrepareEvent>();
        this.GetSystem<ILiveServerSystem>().PostRequestGetRoomId(_token);
    }
}