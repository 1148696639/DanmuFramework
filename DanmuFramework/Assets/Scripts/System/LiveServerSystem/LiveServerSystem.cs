using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QFramework;

public interface ILiveServerSystem : ISystem
{
    /// <summary>
    ///     直播服务开启状态
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    ///     请求直播间数据
    /// </summary>
    /// <param name="token"></param>
    void PostRequestGetRoomId(string token);
}


public class LiveServerSystem : AbstractSystem, ILiveServerSystem
{
    private ClientWebSocket _webSocket;
    public string Token { get; set; }
    public bool IsConnected { get; set; }


    /// <summary>
    ///     通过请求获得房间号
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public void PostRequestGetRoomId(string token)
    {
        var version = this.SendQuery(new GameVersionQuery());
        var body = new JObject
        {
            { "token", token },
            { "clientVersion", version }
        };
        this.GetSystem<IServerCommunicationSystem>().PostRequestAsync("", JsonConvert.SerializeObject(body), Handheld);
    }


    protected override void OnInit()
    {
    }

    private void Handheld(bool isSuccess, string response)
    {
        if (isSuccess)
        {
            DebugCtrl.Log("请求成功！返回：" + response);
            var roomInfo = JObject.Parse(response);
            var roomId = roomInfo["roomId"].ToString();
            var key = roomInfo["anchorId"].ToString();
            DebugCtrl.Log("获取roomId成功！为：" + roomId);
            DebugCtrl.Log("获取主播id成功！为：" + key);
            if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(key))
            {
                DebugCtrl.LogWarning("房间状态异常！");
                this.SendEvent<GameRestartEvent>();
            }

            var configModel = this.GetModel<IGameConfigModel>();
            configModel.Key = key;
            configModel.RoomId = roomId;
            IsConnected = true;
            this.SendEvent<LiveServerOpenSuccessEvent>();
        }
        else
        {
            DebugCtrl.LogWarning(response);
        }
    }
}