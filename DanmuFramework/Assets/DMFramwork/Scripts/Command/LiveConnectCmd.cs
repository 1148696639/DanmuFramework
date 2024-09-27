using QFramework;
namespace DMFramework
{
    namespace Command
    {
        public class LiveConnectCmd : AbstractCommand
        {
            private readonly string _token;

            public LiveConnectCmd(string token)
            {
                _token = token;
            }

            protected override void OnExecute()
            {
                DebugCtrl.Log("连接直播服务器...");
                var config = this.GetModel<IGameConfigModel>();
                this.GetSystem<ILiveServerSystem>().PostRequestGetRoomId(_token, config.GameName, config.GamePlatform);
            }
        }
    }
}