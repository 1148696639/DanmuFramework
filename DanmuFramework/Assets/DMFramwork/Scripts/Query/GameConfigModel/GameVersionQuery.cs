using QFramework;
namespace DMFramework
{
    public class GameVersionQuery : AbstractQuery<string>
    {
        protected override string OnDo()
        {
            return this.GetModel<IGameConfigModel>().Version;
        }
    }
}