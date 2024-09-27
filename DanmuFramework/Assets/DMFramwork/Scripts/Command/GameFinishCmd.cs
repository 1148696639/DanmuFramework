using QFramework;
namespace DMFramework
{
    /// <summary>
    ///   游戏结束时调用
    /// </summary>
    public class GameFinishCmd : AbstractCommand
    {
        protected override void OnExecute()
        {
            DebugCtrl.Log("游戏结束...");
            this.SendEvent<GameFinishEvent>();
        }
    }
}