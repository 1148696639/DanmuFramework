using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using QFramework;

/// <summary>
///     服务器通信系统接口，定义了与服务器进行各种通信操作的方法。
/// </summary>
public interface IServerCommunicationSystem : ISystem
{
    /// <summary>
    ///     websocket是否连接成功
    /// </summary>
    BindableProperty<bool> WebSocketIsConnected { get; set; }

    /// <summary>
    ///     监听websocket发送过来的消息
    /// </summary>
    Action<string> OnReceiveMessageFromWebSocket { get; set; }

    /// <summary>
    ///     发起 GET 请求到指定的 API 端点。
    /// </summary>
    /// <param name="endpoint">API 端点路径。</param>
    /// <param name="callback">请求完成后的回调，包括服务器响应字符串、成功状态和错误信息。</param>
    void GetRequestAsync(string endpoint, Action<bool, string> callback);

    /// <summary>
    ///     发起 POST 请求到指定的 API 端点，并附带 JSON 数据。
    /// </summary>
    /// <param name="endpoint">API 端点路径。</param>
    /// <param name="jsonData">要发送的 JSON 数据。</param>
    /// <param name="callback">请求完成后的回调，包括服务器响应字符串、成功状态和错误信息。</param>
    void PostRequestAsync(string endpoint, string jsonData, Action<bool, string> callback);

    /// <summary>
    ///     发送信息给服务器，通用方法
    /// </summary>
    /// <param Name="method"></param>
    /// <param Name="collection"></param>
    /// <param Name="data"></param>
    /// <param Name="updateDataName"></param>
    void SendMessageToWebsocket(string method, string data = null);

    /// <summary>
    ///     发送post方法，封装
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="jsonData"></param>
    /// <param name="callbackSuccess"></param>
    /// <param name="callbackFail"></param>
    void SendMessageToPost(string methodName, string jsonData, Action<string> callbackSuccess,
        Action<string> callbackFail = null);

    /// <summary>
    ///     发送get方法，封装
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="callbackSuccess"></param>
    /// <param name="callbackFail"></param>
    void SendMessageToGet(string methodName, Action<string> callbackSuccess, Action<string> callbackFail = null,
        Dictionary<string, object> data = null);

    /// <summary>
    ///     无封装，无返回的post请求
    /// </summary>
    /// <param name="path"></param>
    void SendPostNoBack(string path);

    /// <summary>
    ///     获取服务器消息队列
    /// </summary>
    /// <returns></returns>
    BetterQueue<string> GetServerMsgQueue();

    /// <summary>
    ///     Get请求,同步
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="callback"></param>
    void GetRequest(string endpoint, Action<bool, string> callback);
}

/// <summary>
///     发送给web服务器的信息体
/// </summary>
public struct MessageToWeb
{
    public string data;
    public string method;
}

public class HttpResponseData
{
    /// <summary>
    ///     状态码，成功为200，其他为失败
    /// </summary>
    public int code { get; set; }

    /// <summary>
    ///     消息，成功为success，失败为原因
    /// </summary>
    public string msg { get; set; }

    /// <summary>
    ///     返回数据 成功为返回的数据json字符串，失败为null
    /// </summary>
    public object data { get; set; }
}

public class ServerCommunicationSystem : AbstractSystem, IServerCommunicationSystem
{
    private readonly BetterQueue<string> _messageQueue = new();
    private string _baseUrl;
    private Dictionary<string, object> _headers;
    private WebRequestUtils _webRequestUtils;
    private WebSocketClient _webSocketClient;
    private string _webSocketUrl;
    private ILiveServerSystem m_liveServerSystem;

    public BindableProperty<bool> WebSocketIsConnected { get; set; } = new();

    public void GetRequestAsync(string endpoint, Action<bool, string> callback)
    {
        var path = _baseUrl + endpoint;
        _webRequestUtils.GetRequestAsync(path,_headers, responseStr => { HandleResponse(responseStr, callback); });
    }

    public void GetRequest(string endpoint, Action<bool, string> callback)
    {
        var path = _baseUrl + endpoint;
        _webRequestUtils.GetRequest(path, responseStr => { HandleResponse(responseStr, callback); });
    }

    public void PostRequestAsync(string endpoint, string jsonData, Action<bool, string> callback)
    {
        var path = _baseUrl + endpoint;
        _webRequestUtils.PostRequestAsync(path,_headers, jsonData, responseStr => { HandleResponse(responseStr, callback); });
    }

    public void SendMessageToWebsocket(string method, string data = null)
    {
        //通过websocket发送消息,method为方法名，data为数据
        var jsonStr = JsonConvert.SerializeObject(new Dictionary<string, string>
            { { "method", method }, { "data", data } });
        _ = _webSocketClient.SendMessageAsync(jsonStr);
    }

    public void SendMessageToPost(string methodName, string jsonData, Action<string> callbackSuccess,
        Action<string> callbackFail = null)
    {
        var path = "/java/api/game/" + methodName;
        PostRequestAsync(path, jsonData, (isSuccess, responseData) =>
        {
            if (isSuccess)
            {
                callbackSuccess?.Invoke(responseData);
            }
            else
            {
                callbackFail?.Invoke(responseData);
            }
        });
    }


    public void SendMessageToGet(string methodName, Action<string> callbackSuccess, Action<string> callbackFail = null,
        Dictionary<string, object> data = null)
    {
        var path = "/java/" + methodName;
        if (data != null) path += "?" + string.Join("&", data.Select(x => x.Key + "=" + x.Value));
        GetRequestAsync(path, (isSuccess, responseData) =>
        {
            if (isSuccess)
            {
                callbackSuccess?.Invoke(responseData);
            }
            else
            {
                callbackFail?.Invoke(responseData);
            }
        });
    }

    public void SendPostNoBack(string path)
    {
        _webRequestUtils.Post(path);
    }

    public BetterQueue<string> GetServerMsgQueue()
    {
        return _messageQueue;
    }

    public Action<string> OnReceiveMessageFromWebSocket { get; set; }

    private async void CloseWebSocket()
    {
        if (_webSocketClient != null)
            await _webSocketClient.DisconnectAsync();
        else
            DebugCtrl.LogWarning("websocket连接不存在！无法关闭！");
    }


    private void HandleResponse(string responseStr, Action<bool, string> callback)
    {
        var response = JsonConvert.DeserializeObject<HttpResponseData>(responseStr);
        if (response != null)
        {
            var data = JsonConvert.SerializeObject(response.data);
            if (response.code == 200)
                callback?.Invoke(true, data);
            else if (response.code == 503)
                DebugCtrl.LogError("服务器错误：" + response.msg);
            else
                callback?.Invoke(false, response.msg);
        }
    }

    protected override void OnInit()
    {
        this.RegisterEvent<GameConfigInitEvent>(OnGameConfigInit);
        this.RegisterEvent<GameFinishEvent>(OnGameFinish);
        this.RegisterEvent<GamePrepareEvent>(OnGamePrepare);
    }

    private void OnGamePrepare(GamePrepareEvent obj)
    {
        var token = m_liveServerSystem.Token;
        _headers = new Dictionary<string, object>
        {
            { "Authorization", token }
        };
        _webSocketClient =
            new WebSocketClient(_webSocketUrl, _headers, 2, 1);
        _webSocketClient.OnConnected += OnWebSocketConnected;
        _webSocketClient.OnDisconnected += OnWebSocketDisConnected;
        _webSocketClient.OnError += OnWebSocketError;
        _webSocketClient.OnMessageReceived += OnWebSocketReceivedMessage;
        _webSocketClient.OnMessageSent += OnWebSocketSentMessage;
        _webSocketClient.OnReConnect += OnWebSocketReConnect;
        _webSocketClient.ConnectAsync();
    }


    private void OnGameConfigInit(GameConfigInitEvent obj)
    {
        _baseUrl = this.SendQuery(new GameHttpUrlBaseQuery());
        _webSocketUrl = this.SendQuery(new GameWebSocketUrlQuery());
        _webRequestUtils = new WebRequestUtils(5);
        m_liveServerSystem = this.GetSystem<ILiveServerSystem>();
    }


    private void OnGameFinish(GameFinishEvent obj)
    {
        CloseWebSocket();
    }

    private void OnWebSocketReConnect(int index)
    {
        DebugCtrl.Log("websocket正在尝试连接，次数：" + index);
        if (index > 10)
        {
            DebugCtrl.LogError("websocket重试上限！");
            CloseWebSocket();
            this.SendEvent<GameRestartEvent>();
        }
    }

    private void OnWebSocketSentMessage(string obj)
    {
        DebugCtrl.Log("websocket发送消息：" + obj);
    }

    private void OnWebSocketReceivedMessage(string msg)
    {
        //每收到一条信息都将信息存入到对应的队列中，然后再按顺序一个个处理,这样可以保证消息的顺序
        _messageQueue.Enqueue(msg);
    }

    private void OnWebSocketError(Exception obj)
    {
        DebugCtrl.LogError("websocket连接错误，错误原因：" + obj);
    }

    private void OnWebSocketDisConnected()
    {
        DebugCtrl.LogWarning("websocket连接断开!");
    }

    private void OnWebSocketConnected()
    {
        DebugCtrl.Log("websocket连接成功！");
        WebSocketIsConnected.Value = true;
    }
}