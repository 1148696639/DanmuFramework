using System;
using System.IO;
using QFramework;
using UnityEngine;
namespace DMFramework
{
    public class LogCtrl : AbstractController
    {
        // 日志文件存储位置
        public string LogFolder;

        private IGameConfigModel m_GameConfigModel;

        private ITencentServerSystem m_TencentServerSystem;

        private void Awake()
        {
            // 获取日志文件夹路径
            LogFolder = Application.persistentDataPath;

            TypeEventSystem.Global.Register<UploadLogEvent>(OnUploadLog).UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<GameConfigInitEvent>(OnGameInit).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnTencentServerInit(bool obj)
        {
            if (!this.SendQuery(new GameIsTestQuery()))
            {
                LogUploadLastTime();
            }
        }

        private void OnGameInit(GameConfigInitEvent obj)
        {
            m_GameConfigModel = this.GetModel<IGameConfigModel>();
            m_TencentServerSystem = this.GetSystem<ITencentServerSystem>();
            m_TencentServerSystem.IsInit.Register(OnTencentServerInit);
        }

        private void LogUploadLastTime()
        {
            LogUpload("Last", "Player-prev.log");
        }

        private void LogUploadNow()
        {
            LogUpload("Now", "Player.log");
        }

        private void LogUpload(string type, string fileName)
        {
            // 构造日志文件路径
            var timestamp = DateTime.Now.ToString("yyyyMMdHHmmss");
            var localLogName = $"{m_GameConfigModel.Key}_{type}_{timestamp}.log";
            var cosPath = $"{m_GameConfigModel.GameName}/log/{DateTime.Now:yyyyMMdd}/{localLogName}";
            var logPath = LogFolder + $"/{fileName}";
            //检查本地是否有日志文件
            if (!File.Exists(logPath))
            {
                Debug.LogError("日志文件不存在");
                return;
            }
            DebugCtrl.Log("开始上传日志文件");
            m_TencentServerSystem.PutObject("res-1312097821", cosPath, logPath);
        }

        private void OnUploadLog(UploadLogEvent obj)
        {
            LogUploadNow();
        }
    }
}