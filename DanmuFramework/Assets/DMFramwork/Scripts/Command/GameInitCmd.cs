using QFramework;
using UnityEngine;

namespace DMFramework
{
    public class GameInitCmd : AbstractCommand
    {
        private readonly TestInitData _testInitData;
        private readonly GameDataInit m_GameData;
        private readonly bool m_IsTest;


        public GameInitCmd(GameDataInit gameData, TestInitData testInitData, bool isTest)
        {
            m_GameData = gameData;
            _testInitData = testInitData;
            m_IsTest = isTest;
        }

        protected override void OnExecute()
        {
            var gameConfigModel = this.GetModel<IGameConfigModel>();
            gameConfigModel.GameName = m_GameData.GameName;
            gameConfigModel.Version = Application.version;
            gameConfigModel.GamePlatform = m_GameData.GamePlatform;
            gameConfigModel.GameExitIsRegister = m_GameData.GameExitIsRegister;
            if (m_IsTest)
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
                gameConfigModel.HttpUrlBase = m_GameData.HttpUrl;
                gameConfigModel.WebSocketUrl = m_GameData.WebSocketUrl;
            }

            DebugCtrl.Log("游戏初始化...");
            this.SendEvent<GameConfigInitEvent>();
        }
    }
}