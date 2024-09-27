using QFramework;

public class GameIsTestQuery : AbstractQuery<bool>
{
    protected override bool OnDo()
    {
        return this.GetModel<IGameConfigModel>().IsTest;
    }
}