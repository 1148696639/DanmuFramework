using QFramework;
using UnityEngine;

public class GameInitCmd : AbstractCommand
{
    private readonly string _gameName;
    private readonly GamePlatformType _gamePlatform;
    private readonly string _httpUrl;
    private readonly string _webSocketUrl;
    private readonly TestInitData _testInitData;

    /// <summary>
    ///  打开客户端时首先调用数据初始化命令
    /// </summary>
    /// <param name="gameName"></param>
    /// <param name="gamePlatform"></param>
    /// <param name="httpUrl"></param>
    /// <param name="webSocketUrl"></param>
    /// <param name="testInitData"></param>
    public GameInitCmd(string gameName, GamePlatformType gamePlatform, string httpUrl, string webSocketUrl,
        TestInitData testInitData = null)
    {
        _gameName = gameName;
        _gamePlatform = gamePlatform;
        _httpUrl = httpUrl;
        _webSocketUrl = webSocketUrl;
        _testInitData = testInitData;
    }

    protected override void OnExecute()
    {
        var gameConfigModel = this.GetModel<IGameConfigModel>();
        gameConfigModel.GameName = _gameName;
        gameConfigModel.Version = Application.version;
        gameConfigModel.GamePlatform = _gamePlatform;
        if (_testInitData != null)
        {
            gameConfigModel.IsTest = true;
            gameConfigModel.HttpUrlBase = _testInitData.HttpUrlTest;
            gameConfigModel.WebSocketUrl = _testInitData.WebSocketUrlTest;
            gameConfigModel.RoomId = _testInitData.RoomId;
            gameConfigModel.Key = _testInitData.Key;
            gameConfigModel.WebMessageHandleFrequency = _testInitData.WebMessageHandleFrequency;
        }
        else
        {
            gameConfigModel.HttpUrlBase = _httpUrl;
            gameConfigModel.WebSocketUrl = _webSocketUrl;
        }

        DebugCtrl.Log("游戏初始化...");
        this.SendEvent<GameConfigInitEvent>();
    }
}