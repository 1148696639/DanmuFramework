using QFramework;

public class WebSocketIsConnectedQuery : AbstractQuery<bool>
{
    protected override bool OnDo()
    {
        return this.GetSystem<IServerCommunicationSystem>().WebSocketIsConnected;
    }
}