using QFramework;

internal interface IGameConfigModel : IModel
{
    /// <summary>
    ///     游戏名
    /// </summary>
    string GameName { get; set; }

    /// <summary>
    ///     直播间id
    /// </summary>
    string RoomId { get; set; }

    /// <summary>
    ///     直播间的唯一key
    /// </summary>
    string Key { get; set; }

    /// <summary>
    ///     游戏版本
    /// </summary>
    string Version { get; set; }

    /// <summary>
    ///     是否是测试版本
    /// </summary>
    bool IsTest { get; set; }

    /// <summary>
    ///     http请求的基础地址
    /// </summary>
    string HttpUrlBase { get; set; }

    /// <summary>
    ///     websocket链接地址
    /// </summary>
    string WebSocketUrl { get; set; }

    /// <summary>
    ///     直播平台
    /// </summary>
    GamePlatformEnum GamePlatform { get; set; }
}

public enum GamePlatformEnum
{
    Tk,
    X7,
    KS
}

public class GameConfigModel : AbstractModel, IGameConfigModel
{
    public string GameName { get; set; }
    public string RoomId { get; set; }
    public string Key { get; set; }
    public string Version { get; set; }
    public bool IsTest { get; set; }
    public string HttpUrlBase { get; set; }
    public string WebSocketUrl { get; set; }
    public GamePlatformEnum GamePlatform { get; set; }


    protected override void OnInit()
    {
    }
}