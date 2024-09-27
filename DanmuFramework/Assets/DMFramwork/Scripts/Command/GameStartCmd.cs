using QFramework;
namespace DMFramework
{
    /// <summary>
    ///     初始数据加载完成之后调用，正式开始游戏
    /// </summary>
    public class GameStartCmd : AbstractCommand
    {
        protected override void OnExecute()
        {
            DebugCtrl.Log("游戏开始！");
            this.SendEvent(new SceneChangeEvent("Game"));
            this.SendEvent<GameStartEvent>();
        }
    }
}