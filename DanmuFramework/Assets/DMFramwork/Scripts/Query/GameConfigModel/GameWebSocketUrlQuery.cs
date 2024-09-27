using QFramework;
namespace DMFramework
{
    public class GameWebSocketUrlQuery : AbstractQuery<string>
    {
        protected override string OnDo()
        {
            return this.GetModel<IGameConfigModel>().WebSocketUrl;
        }
    }
}