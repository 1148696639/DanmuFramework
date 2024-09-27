using System;
using System.Collections;
using System.Collections.Generic;
using Command;
using QFramework;
using UnityEngine;
using UnityEngine.UI;

public class Login : AbstractController
{
    [Header("登录按钮")] public Button LoginBtn;

    [Header("版本号文本")] public Text VersionTxt;

    [Header("分辨率选择栏")] public Dropdown ResolutionDrd;

    [Header("小七房间号输入框")] public InputField RoomIdInp;

    [Header("需要显示的平台组件，按照枚举的顺序摆放")] public List<GameObject> PlatformGos;

    private GamePlatformType _platform;
    private string _token;

    private void Awake()
    {
        ResolutionDrd.value = PlayerPrefs.GetInt("resolution", 0);
        this.RegisterEvent<GameConfigInitEvent>(OnGameInit);
    }


    private void OnGameInit(GameConfigInitEvent obj)
    {
        _platform = this.GetModel<IGameConfigModel>().GamePlatform;
        PlatformCheck();
        LoginUIInit();
        GetToken();
        ResolutionDrd.onValueChanged.AddListener(OnResolutionChanged);
        LoginBtn.onClick.AddListener(OnLoginBtnOk);
        this.GetSystem<IServerCommunicationSystem>().WebSocketIsConnected.Register(OnWebSocketConnected);
        this.GetSystem<ILiveServerSystem>().IsConnected.Register(OnLiveServerConnected);
    }

    private void OnLiveServerConnected(bool obj)
    {
        LoginBtn.interactable = true;
    }

    /// <summary>
    ///     UI初始化
    /// </summary>
    private void LoginUIInit()
    {
        LoginBtn.interactable = false;
        // 将分辨率选择栏ResolutionDrd初始化有三个选项，780x480、1920x1080，2560x1440
        ResolutionDrd.options = new List<Dropdown.OptionData>
        {
            new("780×480"),
            new("1920×1080"),
            new("2560×1440")
        };

        VersionTxt.text = this.SendQuery(new GameVersionQuery());
    }

    /// <summary>
    ///     平台检查
    /// </summary>
    private void PlatformCheck()
    {
        PlatformGos.ForEach(g => g.SetActive(false));
        PlatformGos[FormatConversionHelper.EnumToInt(_platform)].SetActive(true);
    }

    /// <summary>
    ///     设置分辨率
    /// </summary>
    /// <param name="index"></param>
    private void OnResolutionChanged(int index)
    {
        PlayerPrefs.SetInt("resolution", index);
        var resolution = ResolutionDrd.options[index].text.Split("×");
        Screen.SetResolution(int.Parse(resolution[0]), int.Parse(resolution[1]), false);
    }

    /// <summary>
    ///     获取Token
    /// </summary>
    private void GetToken()
    {
        if (_platform == GamePlatformType.tk)
        {
            RoomIdInp.gameObject.SetActive(false);
            if (this.SendQuery(new GameIsTestQuery()))
                SetToken(
                    this.SendQuery(new GameRoomIdQuery()));
            else
                StartCoroutine(GetAndSetToken("-c"));
        }
        else if (_platform == GamePlatformType.x7)
        {
            RoomIdInp.gameObject.SetActive(true);
            if (this.SendQuery(new GameIsTestQuery()))
                SetToken(this.SendQuery(new GameRoomIdQuery()));
            else
                RoomIdInp.onEndEdit.AddListener(value =>
                {
                    if (value != "") SetToken(value);
                });
        }
        else if (_platform == GamePlatformType.ks)
        {
            RoomIdInp.gameObject.SetActive(false);
            if (this.SendQuery(new GameIsTestQuery()))
                SetToken(this.SendQuery(new GameRoomIdQuery()));
            else
                StartCoroutine(GetAndSetToken("-token="));
        }
    }

    /// <summary>
    ///     当websocket连接成功之后进入游戏
    /// </summary>
    /// <param name="obj"></param>
    private void OnWebSocketConnected(bool obj)
    {
        if (obj) this.SendCommand<GameStartCmd>();
    }

    private void SetToken(string token)
    {
        DebugCtrl.Log("Token：" + _token);
        _token = token;
        this.SendCommand(new LiveConnectCmd(_token));
    }

    /// <summary>
    ///     获得启动参数中的Token
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetAndSetToken(string target)
    {
        string token = null;
        while (token == null)
        {
            DebugCtrl.Log("正在获取Token。。。");
            token = GetTokenFromArgs(target);
            yield return new WaitForSeconds(1);
        }

        SetToken(token);
    }

    // 从启动参数中查找 -token 参数的值
    private string GetTokenFromArgs(string target)
    {
        var args = Environment.GetCommandLineArgs();
        foreach (var str in args)
            if (str.StartsWith(target))
            {
                if (str.Contains('='))
                {
                    return str.Substring(target.Length);
                }

                var index = Array.IndexOf(args, str);
                if (index >= 0 && index < args.Length - 1) return args[index + 1];
            }

        return null;
    }

    /// <summary>
    ///     点击开始
    /// </summary>
    private void OnLoginBtnOk()
    {
        LoginBtn.transform.GetChild(0).GetComponent<Text>().text = "连接中";
        LoginBtn.onClick.RemoveAllListeners();
        this.SendCommand<GamePrepareCmd>();
    }
}