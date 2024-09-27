
using DMFramework;

namespace QFramework
{
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        protected override void Init()
        {
            RegisterModel<IGameConfigModel>(new GameConfigModel());

            RegisterSystem<ILiveServerSystem>(new LiveServerSystem());
            RegisterSystem<IServerCommunicationSystem>(new ServerCommunicationSystem());
            RegisterSystem<ITimeSystem>(new TimeSystem());
            RegisterSystem<ITencentServerSystem>(new TencentServerSystem());
        }
    }
}