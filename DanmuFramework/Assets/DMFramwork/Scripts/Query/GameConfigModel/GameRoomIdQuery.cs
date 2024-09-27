using QFramework;
namespace DMFramework
{
    public class GameRoomIdQuery : AbstractQuery<string>
    {
        protected override string OnDo()
        {
            return this.GetModel<IGameConfigModel>().RoomId;
        }
    }
}