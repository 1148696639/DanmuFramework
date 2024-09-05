/// <summary>
///     1.游戏初始化事件,用于初始化游戏配置等非耗时操作
/// </summary>
public struct GameConfigInitEvent
{
}

/// <summary>
///    2. 游戏准备事件，用于加载游戏资源，切换场景等耗时操作
/// </summary>
public struct GamePrepareEvent
{
}

/// <summary>
///    4. 游戏开始事件，用于正式开始游戏逻辑
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
///     3.直播服务器开启成功事件
/// </summary>
public struct LiveServerOpenSuccessEvent
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