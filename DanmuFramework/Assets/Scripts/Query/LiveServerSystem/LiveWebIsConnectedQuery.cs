using QFramework;

public class LiveWebIsConnectedQuery : AbstractQuery<bool>
{
    protected override bool OnDo()
    {
        return this.GetSystem<ILiveServerSystem>().IsConnected;
    }
}