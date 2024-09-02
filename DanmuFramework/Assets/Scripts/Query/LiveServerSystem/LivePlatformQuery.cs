using QFramework;

public class LivePlatformQuery : AbstractQuery<GamePlatformEnum>
{
    protected override GamePlatformEnum OnDo()
    {
        return this.GetModel<IGameConfigModel>().GamePlatform;
    }
}