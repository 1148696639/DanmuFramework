using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// WebSocket连接程序
/// </summary>
public class WebSocketClient
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly TimeSpan _reconnectDelay;
    private readonly Uri _serverUri;
    private readonly TimeSpan _checkIntervals;

    private ClientWebSocket _clientWebSocket;

    private Dictionary<string, object> _headers;

    // 重新连接计数器
    private int _reconnectCount;
    private bool _shutDown;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serverUri">WebSocket 服务器 URI</param>
    /// <param name="headers">请求头</param>
    /// <param name="reconnectDelay">重新连接的延迟时间</param>
    /// <param name="checkIntervals">自动检查连接状态的时间间隔</param>
    public WebSocketClient(string serverUri, Dictionary<string, object> headers, float reconnectDelay, float checkIntervals)
    {
        _serverUri = new Uri(serverUri);
        _reconnectDelay = TimeSpan.FromSeconds(reconnectDelay);
        _cancellationTokenSource = new CancellationTokenSource();
        _checkIntervals = TimeSpan.FromSeconds(checkIntervals);
        _headers = headers;
    }

    // 事件，用于通知 WebSocket 状态信息
    public event Action<string> OnMessageReceived;
    public event Action<string> OnMessageSent;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<Exception> OnError;
    public event Action<int> OnReConnect;

    /// <summary>
    /// 连接到 WebSocket 服务器
    /// </summary>
    public async Task ConnectAsync()
    {
        _clientWebSocket = new ClientWebSocket();
        await ConnectWebSocketLoop();
    }

    /// <summary>
    /// 循环尝试连接
    /// </summary>
    private async Task ConnectWebSocketLoop()
    {
        while (!_shutDown)
        {
            if (_clientWebSocket.State != WebSocketState.Open)
                try
                {
                    await ConnectWebSocketStart();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex); // 触发错误事件
                    await Task.Delay(_reconnectDelay); // 等待重新连接的时间
                }

            await Task.Delay(_checkIntervals); // 每隔一段时间检查连接状态
        }
    }

    private async Task ConnectWebSocketStart()
    {
        _reconnectCount++;
        OnReConnect?.Invoke(_reconnectCount);
        foreach (var header in _headers)
        {
            var value = header.Value.ToString();
            _clientWebSocket.Options.SetRequestHeader(header.Key, value);
        }
        await _clientWebSocket.ConnectAsync(_serverUri, CancellationToken.None);
        OnConnected?.Invoke(); // 触发连接成功事件
        _reconnectCount = 0;
        await Task.Run(async () => await ReceiveLoop());
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="message">要发送的消息</param>
    public async Task SendMessageAsync(string message)
    {
        // 确保 WebSocket 处于 Open 状态，如果不是，等待状态变为 Open
        while (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
        {
            Debug.LogWarning("等待连接中...");
            await Task.Delay(1000); // 根据需要调整等待时间
        }
        var buffer = Encoding.UTF8.GetBytes(message);
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
        OnMessageSent?.Invoke(message); // 触发消息发送事件
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[1024 * 1024];

        try
        {
            while (_clientWebSocket.State == WebSocketState.Open)
            {
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // 接收消息并通知订阅者
                    DebugCtrl.Log("收到消息：" + message);
                    OnMessageReceived?.Invoke(message); // 触发消息接收事件
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    HandleDisconnection(); // 处理断开连接
                    break;
                }
            }
        }
        catch (Exception e)
        {
            OnError?.Invoke(e); // 触发错误事件
        }
    }

    /// <summary>
    /// 处理断开连接
    /// </summary>
    private void HandleDisconnection()
    {
        OnDisconnected?.Invoke(); // 触发断开连接事件
        _clientWebSocket.Dispose();
    }

    /// <summary>
    /// 主动断开连接
    /// </summary>
    public async Task DisconnectAsync()
    {
        _shutDown = true;
        if (_clientWebSocket.State == WebSocketState.Open)
        {
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect",
                _cancellationTokenSource.Token);
        }
        Dispose();
        OnDisconnected?.Invoke(); // 触发断开连接事件
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    private void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _clientWebSocket?.Dispose();
    }

}
