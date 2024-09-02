using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

public class LogCtrl : AbstractController
{
    // 日志文件名字
    public string localLogName;

    // 日志文件存储位置
    public string LogFolder;

    public string ServerUrl;

    public string upLogFileName;

    public List<Header> Headers;

    private Dictionary<string, string> _headers;

    private bool _isStart;
    private LogUploader _logUploader;

    private void Awake()
    {
        // 获取日志文件夹路径
        LogFolder = Application.persistentDataPath;
        // 构造日志文件路径
        var timestamp = DateTime.Now.ToString("yyyyMMddhhmmss");
        localLogName = $"output_{timestamp}.mlog";
        this.RegisterEvent<UploadLogEvent>(OnUploadLog);
        this.RegisterEvent<GamePrepareEvent>(OnGamePrepare);
    }

    private void Start()
    {
        //将Headers转换为Dictionary
        _headers = new Dictionary<string, string>();
        foreach (var header in Headers) _headers.Add(header.key, header.value);
    }

    private void Update()
    {
        if (_isStart) _logUploader.Update();
    }

    private void OnApplicationQuit()
    {
        if (!this.SendQuery(new GameIsTestQuery()))
            _logUploader.CompressionAndUploadLog();
    }

    private void OnGamePrepare(GamePrepareEvent obj)
    {
        _logUploader = new LogUploader(ServerUrl, upLogFileName, localLogName, _headers);
    }

    private void OnUploadLog(UploadLogEvent obj)
    {
        _logUploader.CompressionAndUploadLog();
    }

    [Serializable]
    //用来序列化Header
    public class Header
    {
        public string key;
        public string value;
    }
}