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
            if (this.SendQuery(new GameExitIsRegisterQuery()))
            {
                this.GetSystem<IServerCommunicationSystem>().SendMessageToWebsocket("CLIENT_EXIT");
            }

            this.SendEvent<GameFinishEvent>();
        }
    }
}