# DanmuFramework - 弹幕游戏开发框架

## 项目概述

DanmuFramework（弹幕框架）是一个基于 Unity 引擎的游戏开发框架，专门用于开发与直播平台集成的互动游戏。该框架实现了与多个直播平台的实时通信，支持弹幕互动、WebSocket 连接和云服务集成。

## 核心特性

### 1. 架构设计
- **QFramework 架构**: 采用改进的 QFramework 架构模式，实现 MVC 分层设计
- **模块化设计**: 清晰的模块划分，包括 Model、System、Command、Query、Event、Utility、ViewCtrl
- **IOC 容器**: 内置依赖注入容器，便于模块间解耦
- **事件系统**: 完整的事件注册、发送和注销机制

### 2. 多平台支持
支持以下直播平台：
- **tk**: 抖音平台
- **x7**: 小七平台
- **ks**: 快手平台

### 3. 核心系统

#### LiveServerSystem (直播服务系统)
- 管理与直播服务器的连接状态
- 处理房间 ID 和主播 Token 认证
- 实时监控连接状态

#### ServerCommunicationSystem (服务器通信系统)
- 处理 HTTP 请求和响应
- 管理 WebSocket 连接
- 实现消息队列和异步通信

#### TimeSystem (时间系统)
- 游戏时间管理
- 计时器功能

#### TencentServerSystem (腾讯云服务系统)
- 集成腾讯云 COS 对象存储
- 文件上传和下载
- 日志管理

### 4. 实用工具库

#### 攻击系统 (AttackHelper)
- `LinearProjectileAttack`: 直线投射物攻击
- `ParabolicProjectileAttack`: 抛物线投射物攻击
- `TrackingMissileAttack`: 追踪导弹攻击
- `MultiProjectileAttack`: 多投射物攻击
- `MeleeAttack`: 近战攻击
- `SplatProjectileAttack`: 溅射投射物攻击

#### 对象池管理
- `BetterPoolManager`: 改进的对象池管理器
- `UIObjectPool`: UI 对象池
- 支持对象复用，提升性能

#### 网络通信
- `WebSocketClient`: WebSocket 客户端实现
- `WebRequestUtils`: HTTP 请求工具
- `UnityMainThreadDispatcher`: 主线程调度器

#### 其他工具
- `AudioManager`: 音频管理器（649 行，功能完整）
- `SkillCtrl`: 技能控制系统
- `LogUploader`: 日志上传工具
- `DownloadHelper`: 下载助手
- `EncryptHelper`: 加密助手
- `DateTimeHelper`: 日期时间助手
- `RandomHelper`: 随机数助手
- `SceneTransitionManager`: 场景转换管理器
- `PerformanceTester`: 性能测试工具

## 项目结构

```
DanmuFramework/
├── DMFramework.unitypackage        # Unity 资源包
└── DanmuFramework/                 # Unity 项目目录
    ├── Assets/
    │   └── DMFramwork/
    │       ├── Plugins/            # 第三方插件
    │       │   ├── DOTween/        # DOTween 动画插件
    │       │   ├── DOTweenPro/     # DOTween Pro
    │       │   ├── PathologicalGames/  # 对象池插件
    │       │   ├── COSXML.dll      # 腾讯云 COS SDK
    │       │   ├── Newtonsoft.Json.dll  # JSON 处理
    │       │   └── 7z.dll          # 压缩工具
    │       ├── Scenes/             # 场景文件
    │       └── Scripts/            # 脚本目录
    │           ├── QFramework/     # QFramework 架构核心
    │           ├── Command/        # 命令模式实现
    │           ├── Model/          # 数据模型
    │           ├── System/         # 系统层
    │           ├── Query/          # 查询层
    │           ├── Event/          # 事件定义
    │           ├── Utility/        # 工具类
    │           ├── ViewCtrl/       # 视图控制器
    │           └── GameArchitecture.cs  # 游戏架构入口
    └── Packages/                   # Unity 包管理
        └── manifest.json
```

## 架构说明

### QFramework 架构分层

```
┌─────────────────────────────────────┐
│         Presentation Layer          │
│  (ViewCtrl - MonoBehaviour)         │
│  - GameManager                      │
│  - Login                            │
│  - WebMessageMgr                    │
└────────────┬────────────────────────┘
             │ Command/Query
┌────────────▼────────────────────────┐
│         Business Layer              │
│  (Command/Query)                    │
│  - GameInitCmd                      │
│  - GameStartCmd                     │
│  - LiveConnectCmd                   │
└────────────┬────────────────────────┘
             │ Access
┌────────────▼────────────────────────┐
│         System Layer                │
│  (System - Business Logic)          │
│  - LiveServerSystem                 │
│  - ServerCommunicationSystem        │
│  - TimeSystem                       │
│  - TencentServerSystem              │
└────────────┬────────────────────────┘
             │ Access
┌────────────▼────────────────────────┐
│         Model Layer                 │
│  (Model - Data)                     │
│  - GameConfigModel                  │
└────────────┬────────────────────────┘
             │ Use
┌────────────▼────────────────────────┐
│         Utility Layer               │
│  (Utility - Pure Functions)         │
│  - WebSocketClient                  │
│  - AudioManager                     │
│  - AttackHelper                     │
└─────────────────────────────────────┘
```

### 关键设计模式

1. **命令模式 (Command Pattern)**
   - 所有游戏操作通过 Command 执行
   - 示例：`GameInitCmd`, `GameStartCmd`, `LiveConnectCmd`

2. **查询模式 (Query Pattern)**
   - 只读数据查询，不改变状态
   - 示例：`GameVersionQuery`, `GameIsTestQuery`

3. **观察者模式 (Observer Pattern)**
   - 事件系统实现组件间解耦
   - 示例：`GameConfigInitEvent`, `GameRestartEvent`

4. **依赖注入 (Dependency Injection)**
   - IOC 容器管理组件依赖
   - 通过接口访问系统和模型

5. **对象池模式 (Object Pool Pattern)**
   - 优化对象创建和销毁
   - 提升游戏性能

## 主要功能流程

### 游戏初始化流程

```
1. GameManager.Start()
   ↓
2. SendCommand<GameInitCmd>
   ↓
3. 配置 GameConfigModel
   ↓
4. SendEvent<GameConfigInitEvent>
   ↓
5. Login.OnGameInit()
   ↓
6. 获取 Token
   ↓
7. SendCommand<LiveConnectCmd>
   ↓
8. 请求房间信息
   ↓
9. WebSocket 连接
   ↓
10. SendCommand<GameStartCmd>
```

### 直播互动流程

```
1. WebSocket 连接建立
   ↓
2. 监听弹幕消息
   ↓
3. WebMessageMgr 处理消息
   ↓
4. 解析弹幕内容
   ↓
5. 触发游戏逻辑
   ↓
6. 反馈到游戏画面
```

## 配置说明

### GameDataInit (游戏数据初始化)
```csharp
public class GameDataInit
{
    public string GameName;              // 游戏名称
    public GamePlatformType GamePlatform;  // 直播平台
    public string HttpUrl;               // HTTP 服务器地址
    public string WebSocketUrl;          // WebSocket 服务器地址
    public bool GameExitIsRegister;      // 是否注册退出通知
}
```

### TestInitData (测试数据配置)
```csharp
public class TestInitData
{
    public string Key;                   // 主播唯一 Key
    public string RoomId;                // 直播间 ID
    public string HttpUrlTest;           // 测试 HTTP 地址
    public string WebSocketUrlTest;      // 测试 WebSocket 地址
    public float WebMessageHandleFrequency;  // 消息处理频率
}
```

## 技术栈

### Unity 引擎
- Unity 2021+ (根据 packages 判断)
- C# 脚本开发

### 第三方库
- **DOTween**: 动画补间库
- **Newtonsoft.Json**: JSON 序列化/反序列化
- **COSXML**: 腾讯云对象存储 SDK
- **PathologicalGames**: 对象池管理
- **7z**: 文件压缩

### 通信协议
- HTTP/HTTPS: RESTful API 通信
- WebSocket: 实时双向通信
- JSON: 数据交换格式

## 代码统计

- **总文件数**: 67 个 C# 文件
- **总代码行数**: 约 6,161 行
- **最大文件**: QFramework.cs (947 行)
- **核心系统**:
  - AudioManager: 649 行
  - SkillCtrl: 416 行
  - ServerCommunicationSystem: 305 行

## 使用场景

此框架适用于以下场景：
1. **直播互动游戏**: 观众通过弹幕与游戏互动
2. **多平台集成**: 同时支持多个直播平台
3. **实时反馈游戏**: 需要低延迟的观众互动
4. **云端存储**: 游戏数据和日志上传到云端
5. **性能监控**: 内置性能测试工具

## 开发建议

### 最佳实践
1. 遵循 QFramework 架构分层
2. 通过 Command 执行业务逻辑
3. 使用 Event 进行模块间通信
4. 利用对象池优化性能
5. 合理使用 BindableProperty 实现数据绑定

### 扩展开发
1. **添加新系统**: 继承 `AbstractSystem`，在 `GameArchitecture.Init()` 中注册
2. **添加新命令**: 继承 `AbstractCommand`，实现 `OnExecute()` 方法
3. **添加新模型**: 继承 `AbstractModel`，定义接口和实现类
4. **添加新工具**: 实现 `IUtility` 接口，注册到架构中

## 注意事项

1. **Token 获取**: 不同平台的 Token 获取方式不同，需要根据启动参数或配置获取
2. **WebSocket 连接**: 需要先获取房间信息才能建立 WebSocket 连接
3. **消息频率**: 可配置消息处理频率，避免过于频繁
4. **日志管理**: 集成了腾讯云 COS，可自动上传日志
5. **测试模式**: 支持测试模式，可直接配置房间信息

## 版本历史

- **最新提交**: 修改环境参数的获取 (558cd6b)
- **项目状态**: 活跃开发中

## 联系方式

- **GitHub**: https://github.com/1148696639/DanmuFramework
- **作者邮箱**: 1148696639@qq.com

## 许可证

请查看项目中的许可证文件了解具体信息。

---

**注**: 本文档基于代码分析生成，如有疑问请参考源代码实现。
