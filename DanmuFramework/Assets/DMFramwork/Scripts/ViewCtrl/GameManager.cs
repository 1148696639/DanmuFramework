
using System;
using QFramework;
using UnityEngine;
namespace DMFramework
{
    public class TestInitData
    {
        public string HttpUrlTest;
        public string Key;
        public string RoomId;
        public string WebSocketUrlTest;
        public float WebMessageHandleFrequency;
    }

    public class GameManager : AbstractController
    {
        [Header("游戏配置-----------")] public string GameName;

        public GamePlatformType GamePlatform;
        public string HttpUrl;
        public string WebSocketUrl;

        [Header("测试---------------")] public bool IsTest;

        public string RoomId;
        public string Key;
        public string HttpUrlTest;
        public string WebSocketUrlTest;
        [Tooltip("处理web消息间隔时间")]public float WebMessageHandleFrequency;

        //脚本执行顺序：先将配置赋值给GameConfigModel，然后发送GameConfigInitEvent事件，
        //然后获取token，点击进入按钮，发送GamePrepare事件，开始请求直播间数据，
        //获得到之后发送LiveServerOpenSuccessEvent事件，开启websocket连接，连接成功后发送GameStart事件

        private void Awake()
        {
            TypeEventSystem.Global.Register<DisableAnchorEvent>(OnDisableAnchor).UnRegisterWhenGameObjectDestroyed(gameObject);
           this.RegisterEvent<GameRestartEvent>(OnGameRestart).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnGameRestart(GameRestartEvent obj)
        {
            this.SendCommand<GameFinishCmd>();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            Application.Quit();
#endif
        }

        private void OnDisableAnchor(DisableAnchorEvent obj)
        {
            this.SendCommand<GameFinishCmd>();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            Application.Quit();
#endif
        }

        private void Start()
        {
            TestInitData testData = null;
            if (IsTest)
                testData = new TestInitData
                {
                    RoomId = RoomId,
                    Key = Key,
                    HttpUrlTest = HttpUrlTest,
                    WebSocketUrlTest = WebSocketUrlTest,
                    WebMessageHandleFrequency = WebMessageHandleFrequency
                };

            this.SendCommand(new GameInitCmd(GameName, GamePlatform, HttpUrl, WebSocketUrl, testData));
        }

        private void OnApplicationQuit()
        {
            this.SendCommand<GameFinishCmd>();
        }
    }
}