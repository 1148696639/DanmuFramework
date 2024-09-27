using System;
using System.Net.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QFramework;

namespace DMFramework
{
    public interface ILiveServerSystem : ISystem
    {
        /// <summary>
        ///     直播服务开启状态
        /// </summary>
        public BindableProperty<bool> IsConnected { get; set; }

        /// <summary>
        ///     开启直播服务之后服务器返回的token
        /// </summary>
        string Token { get; set; }

        /// <summary>
        ///     请求直播开启
        /// </summary>
        /// <param name="token"></param>
        /// <param name="gameName"></param>
        /// <param name="gamePlatform"></param>
        void PostRequestGetRoomId(string token, string gameName, GamePlatformType gamePlatform);
    }

    public class LiveServerSystem : AbstractSystem, ILiveServerSystem
    {
        private ClientWebSocket _webSocket;
        public string Token { get; set; }
        public BindableProperty<bool> IsConnected { get; set; } = new();


        public void PostRequestGetRoomId(string token, string gameName, GamePlatformType gamePlatform)
        {
            var version = this.SendQuery(new GameVersionQuery());
            var body = new JObject
            {
                { "token", token },
                { "clientVersion", version }
            };
            this.GetSystem<IServerCommunicationSystem>().PostRequestAsync($"/game/{gamePlatform.ToString()}/{gameName}",
                JsonConvert.SerializeObject(body), Handheld);
        }


        protected override void OnInit()
        {
        }

        private void Handheld(bool isSuccess, string response)
        {
            if (isSuccess)
            {
                var res = JObject.Parse(response);
                var roomId = res["roomId"].ToString();
                var key = res["anchorId"].ToString();
                var disabled = bool.TryParse(res["disabled"].ToString(), out var result) && result;
                Token = res["token"].ToString();
                if (disabled)
                {
                    DebugCtrl.LogWarning("房间状态异常！");
                    this.SendEvent<GameRestartEvent>();
                }

                var configModel = this.GetModel<IGameConfigModel>();
                configModel.Key = key;
                configModel.RoomId = roomId;
                IsConnected.Value = true;
                DebugCtrl.Log("直播服务开启成功！");
            }
            else
            {
                DebugCtrl.LogWarning(response);
                this.SendEvent<GameRestartEvent>();
            }
        }
    }
}