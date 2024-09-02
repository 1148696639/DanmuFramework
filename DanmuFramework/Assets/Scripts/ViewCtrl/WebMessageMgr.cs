using System.Collections;
using Newtonsoft.Json;
using QFramework;
using UnityEngine;

public class WebMessageMgr : AbstractController
{
    public float HandleFrequency;

    private void Awake()
    {
        this.RegisterEvent<GameStartEvent>(OnGameStart).UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<GameFinishEvent>(OnGameFinish).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnGameFinish(GameFinishEvent obj)
    {
        StopAllCoroutines();
    }

    private void OnGameStart(GameStartEvent obj)
    {
        //开启服务器指令处理
        StartCoroutine(WebMsgHandle());
    }

    private IEnumerator WebMsgHandle()
    {
        var queue = this.GetSystem<IServerCommunicationSystem>().GetServerMsgQueue();
        while (true)
        {
            if (queue.Count > 0)
            {
                var msg = queue.Dequeue();
                OnReceiveMessageFromWebSocket(msg);
            }

            yield return HandleFrequency == 0 ? null : new WaitForSeconds(HandleFrequency);
        }
    }

    private void OnReceiveMessageFromWebSocket(string msg)
    {
        DebugCtrl.Log("执行websocket消息：" + msg);
        var jsonData = JsonConvert.DeserializeObject<ServerToClientMsgDTO>(msg);
        if (jsonData == null)
        {
            DebugCtrl.LogWarning("解析消息为空！");
        }
        else
        {
            var method = jsonData.method;
            var data = jsonData.data;
            StartCoroutine(DirectiveExecute(data, method));
        }
    }

    /// <summary>
    ///     根据服务器消息方法，执行对应的指令
    /// </summary>
    /// <param name="data"></param>
    /// <param name="methodEnum"></param>
    /// <returns></returns>
    private IEnumerator DirectiveExecute(object data, string methodEnum)
    {
        switch (methodEnum)
        {
            case "INIT_GAME":
                // this.SendCommand(new GameInitCmd(gameData));
                DebugCtrl.Log(data.ToString());
                break;
        }

        yield break;
    }

    public class ServerToClientMsgDTO
    {
        /**
            * 数据
            */
        public object data;

        /**
            * 方法类型
            */
        public string method;

        /**
         * 消息id
         */
        public string msgId;

        /**
         * 直播间id
         */
        public string roomId;
    }
}