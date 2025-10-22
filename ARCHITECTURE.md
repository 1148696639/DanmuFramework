# DanmuFramework 架构详解

## 整体架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                         Unity Game                              │
│                     (DanmuFramework)                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────┐      │
│  │           Presentation Layer (ViewCtrl)              │      │
│  │  ┌────────────┐  ┌────────────┐  ┌──────────────┐   │      │
│  │  │ GameManager│  │   Login    │  │WebMessageMgr │   │      │
│  │  └─────┬──────┘  └──────┬─────┘  └──────┬───────┘   │      │
│  │        │ SendCommand    │ RegisterEvent │            │      │
│  └────────┼────────────────┼───────────────┼────────────┘      │
│           │                │               │                   │
│  ┌────────▼────────────────▼───────────────▼────────────┐      │
│  │         Command/Query Layer (Business Logic)         │      │
│  │  ┌──────────┐  ┌───────────┐  ┌─────────────┐       │      │
│  │  │GameInitCmd│  │LiveConnect│  │GameStartCmd │       │      │
│  │  │           │  │   Cmd     │  │             │       │      │
│  │  └─────┬─────┘  └──────┬────┘  └──────┬──────┘       │      │
│  └────────┼────────────────┼──────────────┼──────────────┘      │
│           │ GetSystem      │              │                    │
│  ┌────────▼────────────────▼──────────────▼──────────────┐      │
│  │              System Layer (Business)                 │      │
│  │  ┌────────────────┐  ┌───────────────────────────┐   │      │
│  │  │LiveServerSystem│  │ServerCommunicationSystem  │   │      │
│  │  ├────────────────┤  ├───────────────────────────┤   │      │
│  │  │-IsConnected    │  │-WebSocketIsConnected      │   │      │
│  │  │-Token          │  │-PostRequestAsync          │   │      │
│  │  │-PostRequest... │  │-SendWebSocketMessage      │   │      │
│  │  └────────┬───────┘  └────────┬──────────────────┘   │      │
│  │  ┌────────▼──────┐   ┌────────▼─────────┐           │      │
│  │  │  TimeSystem   │   │TencentServerSys  │           │      │
│  │  └───────────────┘   └──────────────────┘           │      │
│  └────────┬────────────────────┬─────────────────────────┘      │
│           │ GetModel           │ SendEvent                     │
│  ┌────────▼────────────────────▼─────────────────────────┐      │
│  │              Model Layer (Data)                      │      │
│  │  ┌──────────────────────────────────────────────┐    │      │
│  │  │        GameConfigModel (IGameConfigModel)    │    │      │
│  │  ├──────────────────────────────────────────────┤    │      │
│  │  │ - GameName: string                           │    │      │
│  │  │ - RoomId: string                             │    │      │
│  │  │ - Key: string                                │    │      │
│  │  │ - Version: string                            │    │      │
│  │  │ - IsTest: bool                               │    │      │
│  │  │ - HttpUrlBase: string                        │    │      │
│  │  │ - WebSocketUrl: string                       │    │      │
│  │  │ - GamePlatform: GamePlatformType             │    │      │
│  │  │ - WebMessageHandleFrequency: float           │    │      │
│  │  │ - GameExitIsRegister: bool                   │    │      │
│  │  └──────────────────────────────────────────────┘    │      │
│  └──────────┬───────────────────────────────────────────┘      │
│             │                                                  │
│  ┌──────────▼───────────────────────────────────────────┐      │
│  │            Utility Layer (Pure Functions)           │      │
│  │  ┌────────────┐  ┌────────────┐  ┌─────────────┐    │      │
│  │  │AudioManager│  │AttackHelper│  │PoolManager  │    │      │
│  │  └────────────┘  └────────────┘  └─────────────┘    │      │
│  │  ┌────────────┐  ┌────────────┐  ┌─────────────┐    │      │
│  │  │WebSocket   │  │LogUploader │  │SkillCtrl    │    │      │
│  │  │Client      │  │            │  │             │    │      │
│  │  └────────────┘  └────────────┘  └─────────────┘    │      │
│  └────────────────────────────────────────────────────────┘      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
         │                          │                    │
         ▼                          ▼                    ▼
  ┌─────────────┐          ┌──────────────┐    ┌─────────────┐
  │ Live Server │          │  WebSocket   │    │ Tencent COS │
  │  (HTTP)     │          │   Server     │    │   Storage   │
  └─────────────┘          └──────────────┘    └─────────────┘
```

## QFramework 架构核心概念

### 1. Architecture (架构)
负责统一管理和协调所有层级的组件。

```csharp
public class GameArchitecture : Architecture<GameArchitecture>
{
    protected override void Init()
    {
        // 注册 Model
        RegisterModel<IGameConfigModel>(new GameConfigModel());
        
        // 注册 System
        RegisterSystem<ILiveServerSystem>(new LiveServerSystem());
        RegisterSystem<IServerCommunicationSystem>(new ServerCommunicationSystem());
        RegisterSystem<ITimeSystem>(new TimeSystem());
        RegisterSystem<ITencentServerSystem>(new TencentServerSystem());
    }
}
```

### 2. Controller (控制器)
MonoBehaviour 组件，作为表现层的入口。

```csharp
public class GameManager : AbstractController
{
    // 可以使用所有架构能力
    // - SendCommand<T>()
    // - GetSystem<T>()
    // - GetModel<T>()
    // - RegisterEvent<T>()
    // - SendQuery<T>()
}
```

### 3. System (系统)
业务逻辑层，处理具体的业务功能。

```csharp
public interface ILiveServerSystem : ISystem
{
    BindableProperty<bool> IsConnected { get; set; }
    string Token { get; set; }
    void PostRequestGetRoomId(string token, string gameName, GamePlatformType gamePlatform);
}
```

**能力**:
- ✅ GetModel
- ✅ GetUtility
- ✅ GetSystem
- ✅ SendEvent
- ✅ RegisterEvent
- ✅ SendQuery

### 4. Model (模型)
数据层，存储和管理游戏数据。

```csharp
public interface IGameConfigModel : IModel
{
    string GameName { get; set; }
    string RoomId { get; set; }
    // ... 其他配置属性
}
```

**能力**:
- ✅ GetUtility
- ✅ SendEvent

### 5. Command (命令)
封装一次业务操作，用于执行有副作用的操作。

```csharp
public class GameInitCmd : AbstractCommand
{
    protected override void OnExecute()
    {
        var model = this.GetModel<IGameConfigModel>();
        model.GameName = "MyGame";
        this.SendEvent<GameConfigInitEvent>();
    }
}
```

**能力**:
- ✅ GetSystem
- ✅ GetModel
- ✅ GetUtility
- ✅ SendEvent
- ✅ SendCommand
- ✅ SendQuery

### 6. Query (查询)
封装一次查询操作，只读不改变状态。

```csharp
public class GameVersionQuery : AbstractQuery<string>
{
    protected override string OnDo()
    {
        return this.GetModel<IGameConfigModel>().Version;
    }
}
```

**能力**:
- ✅ GetModel
- ✅ GetSystem
- ✅ SendQuery

### 7. Utility (工具)
纯函数工具类，无状态，无依赖。

```csharp
public class EncryptHelper : IUtility
{
    public static string Encrypt(string text) { /* ... */ }
}
```

**能力**:
- ❌ 无架构依赖（纯工具类）

## 事件系统架构

```
┌─────────────────────────────────────────────────────┐
│              TypeEventSystem                        │
│                                                     │
│  ┌──────────────────────────────────────────┐      │
│  │         Event Registry                    │      │
│  │  Dictionary<Type, IEasyEvent>             │      │
│  │                                            │      │
│  │  - GameConfigInitEvent                     │      │
│  │  - GameRestartEvent                        │      │
│  │  - LiveServerOpenSuccessEvent              │      │
│  │  - DisableAnchorEvent                      │      │
│  └──────────────────────────────────────────┘      │
│                                                     │
│  Register<T>(Action<T> callback)                    │
│      │                                              │
│      ├──► Add to event registry                     │
│      └──► Return IUnRegister                        │
│                                                     │
│  Send<T>(T event)                                   │
│      │                                              │
│      └──► Trigger all registered callbacks          │
│                                                     │
│  UnRegister<T>(Action<T> callback)                  │
│      │                                              │
│      └──► Remove from event registry                │
└─────────────────────────────────────────────────────┘
```

### 事件生命周期管理

```csharp
// 1. 注册事件 - 带自动注销
this.RegisterEvent<GameConfigInitEvent>(OnGameInit)
    .UnRegisterWhenGameObjectDestroyed(gameObject);

// 2. 触发事件
this.SendEvent<GameConfigInitEvent>();

// 3. 自动注销（GameObject 销毁时）
```

## 数据绑定架构 (BindableProperty)

```
┌──────────────────────────────────────────────┐
│        BindableProperty<T>                   │
│                                              │
│  ┌────────────┐                              │
│  │   mValue   │  (private field)             │
│  └────────────┘                              │
│        │                                     │
│        ├──► Get: return mValue               │
│        │                                     │
│        └──► Set: if changed                  │
│                  ├─► mValue = newValue       │
│                  └─► Trigger mOnValueChanged │
│                                              │
│  ┌────────────────────────────────┐          │
│  │  mOnValueChanged: Action<T>    │          │
│  └────────────────────────────────┘          │
│        │                                     │
│        ├──► Register(callback)               │
│        │     └─► mOnValueChanged += callback │
│        │                                     │
│        └──► UnRegister(callback)             │
│              └─► mOnValueChanged -= callback │
└──────────────────────────────────────────────┘
```

### 使用示例

```csharp
// System 中定义
public BindableProperty<bool> IsConnected { get; set; } = new();

// Controller 中监听
this.GetSystem<ILiveServerSystem>()
    .IsConnected
    .Register(OnConnectionChanged)
    .UnRegisterWhenGameObjectDestroyed(gameObject);

// 值改变时自动触发
private void OnConnectionChanged(bool isConnected)
{
    Debug.Log($"Connection: {isConnected}");
}
```

## IOC 容器架构

```
┌─────────────────────────────────────────────┐
│            IOCContainer                     │
│                                             │
│  Dictionary<Type, object> mInstances        │
│                                             │
│  ┌────────────────────────────────┐         │
│  │  Type: ILiveServerSystem       │         │
│  │  Instance: LiveServerSystem    │         │
│  ├────────────────────────────────┤         │
│  │  Type: IGameConfigModel        │         │
│  │  Instance: GameConfigModel     │         │
│  ├────────────────────────────────┤         │
│  │  Type: ITimeSystem             │         │
│  │  Instance: TimeSystem          │         │
│  └────────────────────────────────┘         │
│                                             │
│  Register<T>(T instance)                    │
│      └──► mInstances[typeof(T)] = instance  │
│                                             │
│  T Get<T>()                                 │
│      └──► return mInstances[typeof(T)] as T │
└─────────────────────────────────────────────┘
```

## 对象池架构

```
┌───────────────────────────────────────────────────┐
│         BetterPoolManager                         │
│                                                   │
│  Dictionary<string, Stack<GameObject>> pools      │
│                                                   │
│  ┌──────────────────────────────┐                 │
│  │ Pool: "Bullet"               │                 │
│  │   Stack: [obj1, obj2, obj3]  │                 │
│  ├──────────────────────────────┤                 │
│  │ Pool: "Enemy"                │                 │
│  │   Stack: [obj1, obj2]        │                 │
│  └──────────────────────────────┘                 │
│                                                   │
│  Get(prefab)                                      │
│      ├──► if pool has object                      │
│      │     └──► Pop from stack                    │
│      └──► else                                    │
│            └──► Instantiate new                   │
│                                                   │
│  Return(obj)                                      │
│      └──► Push to stack                           │
│                                                   │
│  Clear()                                          │
│      └──► Destroy all pooled objects              │
└───────────────────────────────────────────────────┘
```

## 网络通信架构

### HTTP 通信流程

```
Controller/Command
       │
       │ PostRequestAsync()
       ▼
ServerCommunicationSystem
       │
       │ WebRequestUtils.PostAsync()
       ▼
UnityWebRequest
       │
       │ HTTP POST
       ▼
   Live Server
       │
       │ Response (JSON)
       ▼
ServerCommunicationSystem
       │
       │ Parse & Callback
       ▼
Controller/Command
```

### WebSocket 通信流程

```
Controller
       │
       │ Connect()
       ▼
WebSocketClient
       │
       │ ClientWebSocket.ConnectAsync()
       ▼
WebSocket Server
       │
       │ Receive Messages
       ▼
WebSocketClient
       │
       │ Parse Message
       ▼
WebMessageMgr
       │
       │ Route to Handlers
       ▼
Game Logic
```

## 消息处理架构

```
┌────────────────────────────────────────────────┐
│          WebMessageMgr                         │
│                                                │
│  Queue<WebMessage> messageQueue                │
│                                                │
│  ┌─────────────────────────────────┐           │
│  │ WebSocket Receive                │           │
│  │        │                         │           │
│  │        ▼                         │           │
│  │  Parse JSON Message              │           │
│  │        │                         │           │
│  │        ▼                         │           │
│  │  Enqueue to messageQueue         │           │
│  └─────────────────────────────────┘           │
│                                                │
│  ┌─────────────────────────────────┐           │
│  │ Update() - Every Frame or       │           │
│  │ Controlled Frequency             │           │
│  │        │                         │           │
│  │        ▼                         │           │
│  │  Dequeue Message                 │           │
│  │        │                         │           │
│  │        ▼                         │           │
│  │  Route to Handler                │           │
│  │        │                         │           │
│  │        ▼                         │           │
│  │  Execute Game Logic              │           │
│  └─────────────────────────────────┘           │
└────────────────────────────────────────────────┘
```

## 游戏生命周期

```
┌──────────────┐
│  GameManager │
│   Awake()    │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ Register     │
│ Events       │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│ GameManager  │
│  Start()     │
└──────┬───────┘
       │
       ▼
┌──────────────────┐
│ GameInitCmd      │
│ - Setup Config   │
│ - Send Event     │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ Login.OnGameInit │
│ - UI Setup       │
│ - Get Token      │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ LiveConnectCmd   │
│ - Request Room   │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ Room Info OK     │
│ - Connect WS     │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ GamePrepareCmd   │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ WS Connected     │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ GameStartCmd     │
│ - Start Game     │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ Game Running     │
│ - Handle Events  │
│ - Process Danmu  │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│ GameFinishCmd    │
│ - Cleanup        │
│ - Disconnect     │
└──────────────────┘
```

## 攻击系统架构

```
┌──────────────────────────────────────────────────┐
│           AttackBehaviorFactory                  │
│                                                  │
│  CreateAttackBehavior(AttackConfig)              │
│       │                                          │
│       ├──► LinearProjectileAttack                │
│       ├──► ParabolicProjectileAttack             │
│       ├──► TrackingMissileAttack                 │
│       ├──► MultiProjectileAttack                 │
│       ├──► MeleeAttack                           │
│       └──► SplatProjectileAttack                 │
└──────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────────┐
│           IAttackBehavior                        │
│                                                  │
│  - Execute(source, target, onHit)                │
│  - Stop()                                        │
│  - Pause()                                       │
│  - Resume()                                      │
└──────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────────────┐
│           AttackCtrl                             │
│                                                  │
│  - Manages multiple attacks                      │
│  - Pools projectiles                             │
│  - Handles collision detection                   │
└──────────────────────────────────────────────────┘
```

## 日志系统架构

```
┌─────────────────────────────────────┐
│        DebugCtrl                    │
│                                     │
│  - Log(message)                     │
│  - LogWarning(message)              │
│  - LogError(message)                │
└──────────┬──────────────────────────┘
           │
           ▼
┌─────────────────────────────────────┐
│       LogUploader                   │
│                                     │
│  - Collect logs                     │
│  - Compress with 7z                 │
│  - Upload to Tencent COS            │
└──────────┬──────────────────────────┘
           │
           ▼
┌─────────────────────────────────────┐
│    TencentServerSystem              │
│                                     │
│  - PutObject(bucket, key, file)     │
│  - GetObject(bucket, key)           │
└─────────────────────────────────────┘
```

## 总结

DanmuFramework 采用清晰的分层架构，通过 QFramework 实现了：

1. **松耦合**: 各层通过接口交互，依赖倒置
2. **高内聚**: 每层职责明确，单一职责原则
3. **易扩展**: 通过 Command/Query 模式易于添加新功能
4. **可测试**: 业务逻辑与 Unity 组件分离
5. **可维护**: 代码结构清晰，易于理解和修改

这种架构特别适合需要与外部服务（如直播平台）集成的复杂游戏项目。
