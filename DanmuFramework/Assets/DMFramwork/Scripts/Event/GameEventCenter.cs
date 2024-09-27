namespace DMFramework
{
    /// <summary>
    ///     1.游戏初始化事件（打开客户端自动执行，此时已经获得了客户端配置gameConfig信息）,用于初始化游戏配置,获取本地token开始请求直播数据服务器开通
    /// </summary>
    public struct GameConfigInitEvent
    {
    }

    /// <summary>
    ///    2. 游戏准备事件(需要登陆界面点击)，用于加载游戏资源，切换场景等耗时操作,websocket开始链接
    /// </summary>
    public struct GamePrepareEvent
    {
    }

    /// <summary>
    ///    3. 游戏开始事件，用于正式开始游戏逻辑
    /// </summary>
    public struct GameStartEvent
    {
    }

    /// <summary>
    ///     游戏结束事件
    /// </summary>
    public struct GameFinishEvent
    {
    }

    /// <summary>
    ///     游戏重启事件
    /// </summary>
    public struct GameRestartEvent
    {
    }

    /// <summary>
    ///    日志上传事件
    /// </summary>
    public struct UploadLogEvent
    {
    }

    /// <summary>
    ///   切换场景事件
    /// </summary>
    public struct SceneChangeEvent
    {
        public SceneChangeEvent(string sceneName)
        {
            SceneName = sceneName;
        }
        public string SceneName;
    }

    public struct WebMessageExecuteEvent
    {
        public WebMessageExecuteEvent(string method, object data)
        {
            Method = method;
            Data = data;
        }

        public string Method;
        public object Data;
    }
}