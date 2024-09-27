using QFramework;
namespace DMFramework
{
    public class GameKeyQuery : AbstractQuery<string>
    {
        protected override string OnDo()
        {
            return this.GetModel<IGameConfigModel>().Key;
        }
    }
}