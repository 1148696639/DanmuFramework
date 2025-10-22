# DanmuFramework API 文档

## 核心系统 API

### ILiveServerSystem - 直播服务系统

负责管理与直播服务器的连接和房间信息。

#### 属性

```csharp
// 直播服务连接状态
BindableProperty<bool> IsConnected { get; set; }

// 服务器返回的认证 Token
string Token { get; set; }
```

#### 方法

```csharp
// 请求获取房间 ID
void PostRequestGetRoomId(
    string token,              // 认证 Token
    string gameName,           // 游戏名称
    GamePlatformType platform  // 直播平台类型
)
```

**使用示例**:
```csharp
var liveSystem = this.GetSystem<ILiveServerSystem>();
liveSystem.PostRequestGetRoomId("your_token", "GameName", GamePlatformType.tk);

// 监听连接状态
liveSystem.IsConnected.Register(isConnected => {
    Debug.Log($"直播服务状态: {isConnected}");
});
```

---

### IServerCommunicationSystem - 服务器通信系统

处理所有网络通信，包括 HTTP 请求和 WebSocket 连接。

#### 属性

```csharp
// WebSocket 连接状态
BindableProperty<bool> WebSocketIsConnected { get; set; }
```

#### 方法

```csharp
// 异步 POST 请求
void PostRequestAsync(
    string url,                    // 请求 URL
    string body,                   // 请求体 (JSON)
    Action<bool, string> callback  // 回调 (成功标志, 响应内容)
)

// 异步 GET 请求
void GetRequestAsync(
    string url,                    // 请求 URL
    Action<bool, string> callback  // 回调
)

// 发送 WebSocket 消息
void SendWebSocketMessage(string message)

// 连接 WebSocket
void ConnectWebSocket(string url)

// 断开 WebSocket
void DisconnectWebSocket()
```

**使用示例**:
```csharp
var commSystem = this.GetSystem<IServerCommunicationSystem>();

// HTTP POST 请求
commSystem.PostRequestAsync(
    "/api/game/info",
    JsonConvert.SerializeObject(new { id = 123 }),
    (success, response) => {
        if (success) {
            Debug.Log($"响应: {response}");
        }
    }
);

// WebSocket 连接
commSystem.ConnectWebSocket("wss://example.com/ws");

// 监听连接状态
commSystem.WebSocketIsConnected.Register(isConnected => {
    Debug.Log($"WebSocket 状态: {isConnected}");
});
```

---

### ITencentServerSystem - 腾讯云服务系统

提供腾讯云 COS 对象存储服务的集成。

#### 方法

```csharp
// 上传文件到 COS
void PutObject(
    string bucket,    // 存储桶名称
    string key,       // 对象键（路径）
    string filePath   // 本地文件路径
)

// 下载文件从 COS
void GetObject(
    string bucket,    // 存储桶名称
    string key,       // 对象键
    string savePath   // 保存路径
)

// 获取存储桶列表
Task<BucketResult> GetBulletResult(
    string bucket,          // 存储桶名称
    string prefix,          // 前缀
    bool delimiter = false  // 是否使用分隔符
)

// 下载文件夹中的所有文件
void GetFilesInFolder(
    string bucket,              // 存储桶名称
    string folder,              // 文件夹路径
    string savePath,            // 保存路径
    Action<bool> callback       // 完成回调
)
```

**使用示例**:
```csharp
var tencentSystem = this.GetSystem<ITencentServerSystem>();

// 上传日志文件
tencentSystem.PutObject(
    "my-bucket",
    "logs/game.log",
    Application.persistentDataPath + "/Player.log"
);

// 下载配置文件
tencentSystem.GetObject(
    "my-bucket",
    "configs/game.json",
    Application.dataPath + "/config.json"
);
```

---

### ITimeSystem - 时间系统

提供游戏时间管理功能。

#### 属性

```csharp
// 游戏开始时间
float GameStartTime { get; }

// 游戏运行时间
float GameRunTime { get; }
```

#### 方法

```csharp
// 启动计时
void StartTimer()

// 停止计时
void StopTimer()

// 重置计时
void ResetTimer()

// 获取格式化时间
string GetFormattedTime()
```

**使用示例**:
```csharp
var timeSystem = this.GetSystem<ITimeSystem>();

// 开始计时
timeSystem.StartTimer();

// 获取运行时间
float elapsed = timeSystem.GameRunTime;

// 格式化显示
string timeText = timeSystem.GetFormattedTime(); // "00:15:30"
```

---

## 数据模型 API

### IGameConfigModel - 游戏配置模型

存储游戏的全局配置信息。

#### 属性

```csharp
// 游戏名称
string GameName { get; set; }

// 直播间 ID
string RoomId { get; set; }

// 主播唯一标识
string Key { get; set; }

// 游戏版本
string Version { get; set; }

// 是否测试模式
bool IsTest { get; set; }

// HTTP 服务器地址
string HttpUrlBase { get; set; }

// WebSocket 服务器地址
string WebSocketUrl { get; set; }

// 直播平台类型
GamePlatformType GamePlatform { get; set; }

// Web 消息处理频率（秒）
float WebMessageHandleFrequency { get; set; }

// 是否注册游戏退出通知
bool GameExitIsRegister { get; set; }
```

**使用示例**:
```csharp
var config = this.GetModel<IGameConfigModel>();

// 读取配置
string gameName = config.GameName;
bool isTest = config.IsTest;

// 修改配置
config.WebMessageHandleFrequency = 0.5f; // 每 0.5 秒处理一次消息
```

---

## 命令 API

### GameInitCmd - 游戏初始化命令

初始化游戏配置和系统。

```csharp
public GameInitCmd(
    GameDataInit gameData,      // 游戏数据配置
    TestInitData testInitData,  // 测试数据配置
    bool isTest                 // 是否测试模式
)
```

**使用示例**:
```csharp
var gameData = new GameDataInit {
    GameName = "MyGame",
    GamePlatform = GamePlatformType.tk,
    HttpUrl = "https://api.example.com",
    WebSocketUrl = "wss://ws.example.com",
    GameExitIsRegister = true
};

this.SendCommand(new GameInitCmd(gameData, testData, false));
```

---

### GameStartCmd - 游戏开始命令

启动游戏主循环。

```csharp
this.SendCommand<GameStartCmd>();
```

---

### GamePrepareCmd - 游戏准备命令

准备游戏资源和连接。

```csharp
this.SendCommand<GamePrepareCmd>();
```

---

### GameFinishCmd - 游戏结束命令

清理资源并断开连接。

```csharp
this.SendCommand<GameFinishCmd>();
```

---

### LiveConnectCmd - 直播连接命令

连接到直播服务器。

```csharp
public LiveConnectCmd(string token)  // 认证 Token

this.SendCommand(new LiveConnectCmd("your_token"));
```

---

## 查询 API

### GameVersionQuery - 游戏版本查询

获取当前游戏版本。

```csharp
string version = this.SendQuery(new GameVersionQuery());
// 返回: "1.0.0"
```

---

### GameIsTestQuery - 测试模式查询

检查是否处于测试模式。

```csharp
bool isTest = this.SendQuery(new GameIsTestQuery());
```

---

### GameRoomIdQuery - 房间 ID 查询

获取当前直播间 ID。

```csharp
string roomId = this.SendQuery(new GameRoomIdQuery());
```

---

## 事件 API

### GameConfigInitEvent - 游戏配置初始化事件

游戏配置完成初始化时触发。

```csharp
// 注册监听
this.RegisterEvent<GameConfigInitEvent>(OnGameConfigInit)
    .UnRegisterWhenGameObjectDestroyed(gameObject);

private void OnGameConfigInit(GameConfigInitEvent e)
{
    Debug.Log("游戏配置已初始化");
}

// 触发事件
this.SendEvent<GameConfigInitEvent>();
```

---

### GameRestartEvent - 游戏重启事件

需要重启游戏时触发。

```csharp
this.RegisterEvent<GameRestartEvent>(OnGameRestart);

private void OnGameRestart(GameRestartEvent e)
{
    // 重启游戏逻辑
}
```

---

### DisableAnchorEvent - 主播禁用事件

主播被禁用时触发。

```csharp
TypeEventSystem.Global.Register<DisableAnchorEvent>(OnDisableAnchor)
    .UnRegisterWhenGameObjectDestroyed(gameObject);
```

---

## 工具类 API

### AudioManager - 音频管理器

管理游戏中的音频播放。

#### 方法

```csharp
// 播放背景音乐
void PlayBGM(
    AudioClip clip,         // 音频片段
    float volume = 1f,      // 音量 (0-1)
    bool loop = true        // 是否循环
)

// 播放音效
void PlaySFX(
    AudioClip clip,         // 音频片段
    float volume = 1f       // 音量
)

// 停止背景音乐
void StopBGM()

// 暂停背景音乐
void PauseBGM()

// 恢复背景音乐
void ResumeBGM()

// 设置背景音乐音量
void SetBGMVolume(float volume)

// 设置音效音量
void SetSFXVolume(float volume)

// 渐入渐出
void FadeIn(float duration)
void FadeOut(float duration)
```

**使用示例**:
```csharp
// 播放背景音乐
AudioManager.Instance.PlayBGM(bgmClip, 0.8f, true);

// 播放音效
AudioManager.Instance.PlaySFX(clickSound, 1f);

// 渐出背景音乐
AudioManager.Instance.FadeOut(2f);
```

---

### BetterPoolManager - 对象池管理器

高效的游戏对象池管理。

#### 方法

```csharp
// 从池中获取对象
GameObject Get(
    GameObject prefab,      // 预制体
    Vector3 position,       // 位置
    Quaternion rotation     // 旋转
)

// 归还对象到池
void Return(GameObject obj)

// 预热池
void Prewarm(GameObject prefab, int count)

// 清空池
void Clear(GameObject prefab)

// 清空所有池
void ClearAll()
```

**使用示例**:
```csharp
// 获取对象
GameObject bullet = BetterPoolManager.Instance.Get(
    bulletPrefab,
    firePoint.position,
    firePoint.rotation
);

// 归还对象
BetterPoolManager.Instance.Return(bullet);

// 预热对象池
BetterPoolManager.Instance.Prewarm(bulletPrefab, 50);
```

---

### WebSocketClient - WebSocket 客户端

WebSocket 通信客户端。

#### 方法

```csharp
// 连接
Task ConnectAsync(string url)

// 发送消息
Task SendAsync(string message)

// 接收消息
Task<string> ReceiveAsync()

// 断开连接
Task DisconnectAsync()

// 检查连接状态
bool IsConnected { get; }
```

**使用示例**:
```csharp
var ws = new WebSocketClient();

// 连接
await ws.ConnectAsync("wss://example.com/ws");

// 发送消息
await ws.SendAsync(JsonConvert.SerializeObject(new { type = "ping" }));

// 接收消息
string message = await ws.ReceiveAsync();
```

---

### SkillCtrl - 技能控制器

管理游戏技能系统。

#### 方法

```csharp
// 注册技能
void RegisterSkill(
    string skillId,         // 技能 ID
    ISkill skill            // 技能实例
)

// 使用技能
bool UseSkill(
    string skillId,         // 技能 ID
    GameObject target       // 目标对象
)

// 检查技能是否可用
bool IsSkillReady(string skillId)

// 获取技能冷却时间
float GetSkillCooldown(string skillId)

// 取消技能
void CancelSkill(string skillId)
```

**使用示例**:
```csharp
// 注册技能
SkillCtrl.Instance.RegisterSkill("fireball", new FireballSkill());

// 使用技能
if (SkillCtrl.Instance.UseSkill("fireball", enemy)) {
    Debug.Log("技能释放成功");
}

// 检查冷却
float cooldown = SkillCtrl.Instance.GetSkillCooldown("fireball");
```

---

### AttackHelper - 攻击助手

提供多种攻击行为实现。

#### AttackBehaviorFactory

```csharp
// 创建攻击行为
IAttackBehavior CreateAttackBehavior(AttackConfig config)
```

#### AttackConfig

```csharp
public class AttackConfig
{
    public AttackType Type;          // 攻击类型
    public float Damage;             // 伤害值
    public float Speed;              // 移动速度
    public float Range;              // 攻击范围
    public GameObject ProjectilePrefab; // 投射物预制体
    public LayerMask TargetLayer;    // 目标层
}
```

**使用示例**:
```csharp
var config = new AttackConfig {
    Type = AttackType.Linear,
    Damage = 10f,
    Speed = 20f,
    Range = 100f,
    ProjectilePrefab = bulletPrefab,
    TargetLayer = enemyLayer
};

var attack = AttackBehaviorFactory.CreateAttackBehavior(config);
attack.Execute(transform.position, target.position, OnHit);
```

---

### LogUploader - 日志上传器

自动收集和上传游戏日志。

#### 方法

```csharp
// 开始收集日志
void StartCollecting()

// 停止收集
void StopCollecting()

// 上传日志
Task UploadLogs(
    string bucket,          // 存储桶
    string path             // 上传路径
)

// 清空本地日志
void ClearLocalLogs()
```

**使用示例**:
```csharp
// 开始收集
LogUploader.Instance.StartCollecting();

// 上传日志
await LogUploader.Instance.UploadLogs("my-bucket", "logs/");
```

---

### DebugCtrl - 调试控制器

统一的日志输出接口。

#### 方法

```csharp
// 普通日志
static void Log(string message)

// 警告日志
static void LogWarning(string message)

// 错误日志
static void LogError(string message)

// 条件日志（仅在测试模式下输出）
static void LogTest(string message)
```

**使用示例**:
```csharp
DebugCtrl.Log("游戏开始");
DebugCtrl.LogWarning("资源加载较慢");
DebugCtrl.LogError("网络连接失败");
```

---

### DownloadHelper - 下载助手

处理文件下载。

#### 方法

```csharp
// 下载文件
Task<bool> DownloadFile(
    string url,             // 下载 URL
    string savePath,        // 保存路径
    Action<float> onProgress // 进度回调 (0-1)
)

// 下载文本
Task<string> DownloadText(string url)

// 下载贴图
Task<Texture2D> DownloadTexture(string url)
```

**使用示例**:
```csharp
bool success = await DownloadHelper.DownloadFile(
    "https://example.com/file.zip",
    Application.dataPath + "/file.zip",
    progress => {
        Debug.Log($"下载进度: {progress * 100}%");
    }
);
```

---

### EncryptHelper - 加密助手

提供加密和解密功能。

#### 方法

```csharp
// MD5 加密
static string MD5Encrypt(string text)

// Base64 编码
static string Base64Encode(string text)

// Base64 解码
static string Base64Decode(string text)
```

**使用示例**:
```csharp
string encrypted = EncryptHelper.MD5Encrypt("password");
string encoded = EncryptHelper.Base64Encode("data");
```

---

### DateTimeHelper - 日期时间助手

时间相关的工具方法。

#### 方法

```csharp
// 获取当前时间戳
static long GetTimestamp()

// 时间戳转 DateTime
static DateTime TimestampToDateTime(long timestamp)

// DateTime 转时间戳
static long DateTimeToTimestamp(DateTime dateTime)

// 格式化时间
static string FormatTime(float seconds)  // 返回 "HH:MM:SS"
```

**使用示例**:
```csharp
long now = DateTimeHelper.GetTimestamp();
string timeStr = DateTimeHelper.FormatTime(125.5f); // "00:02:05"
```

---

### RandomHelper - 随机助手

随机数生成工具。

#### 方法

```csharp
// 随机整数
static int Range(int min, int max)

// 随机浮点数
static float Range(float min, float max)

// 随机布尔值
static bool RandomBool()

// 从数组中随机选择
static T RandomElement<T>(T[] array)

// 打乱数组
static void Shuffle<T>(T[] array)
```

**使用示例**:
```csharp
int randomInt = RandomHelper.Range(1, 100);
float randomFloat = RandomHelper.Range(0f, 1f);
string randomItem = RandomHelper.RandomElement(itemArray);
```

---

### SceneTransitionManager - 场景转换管理器

管理场景切换和过渡效果。

#### 方法

```csharp
// 加载场景
void LoadScene(
    string sceneName,           // 场景名称
    Action onComplete = null    // 完成回调
)

// 带淡入淡出效果加载场景
void LoadSceneWithFade(
    string sceneName,
    float fadeDuration = 0.5f
)

// 异步加载场景
AsyncOperation LoadSceneAsync(
    string sceneName,
    Action<float> onProgress    // 进度回调
)
```

**使用示例**:
```csharp
// 简单加载
SceneTransitionManager.Instance.LoadScene("MainGame");

// 带淡入淡出
SceneTransitionManager.Instance.LoadSceneWithFade("MainGame", 1f);

// 异步加载显示进度
SceneTransitionManager.Instance.LoadSceneAsync("MainGame", progress => {
    loadingBar.fillAmount = progress;
});
```

---

### PerformanceTester - 性能测试器

测试和监控游戏性能。

#### 方法

```csharp
// 开始测试
void StartTest(int targetFPS = 60)

// 停止测试
void StopTest()

// 获取当前 FPS
float GetCurrentFPS()

// 获取平均 FPS
float GetAverageFPS()

// 获取最低 FPS
float GetMinFPS()

// 获取最高 FPS
float GetMaxFPS()
```

**使用示例**:
```csharp
// 开始性能测试
PerformanceTester.Instance.StartTest(60);

// 获取性能数据
float currentFPS = PerformanceTester.Instance.GetCurrentFPS();
float avgFPS = PerformanceTester.Instance.GetAverageFPS();
```

---

## 扩展方法

### BindableProperty 扩展

```csharp
// 注册并在 GameObject 销毁时自动注销
IUnRegister UnRegisterWhenGameObjectDestroyed(
    this IUnRegister self,
    GameObject gameObject
)

// 注册并在 GameObject 禁用时自动注销
IUnRegister UnRegisterWhenGameObjectDisable(
    this IUnRegister self,
    GameObject gameObject
)
```

**使用示例**:
```csharp
property.Register(OnValueChanged)
    .UnRegisterWhenGameObjectDestroyed(gameObject);
```

---

## 平台枚举

### GamePlatformType

```csharp
public enum GamePlatformType
{
    tk,  // 抖音
    x7,  // 小七
    ks   // 快手
}
```

---

## 完整示例

### 创建一个简单的游戏控制器

```csharp
using QFramework;
using DMFramework;

public class MyGameController : AbstractController
{
    private void Start()
    {
        // 初始化游戏
        InitGame();
        
        // 注册事件
        RegisterEvents();
        
        // 连接服务器
        ConnectToServer();
    }
    
    private void InitGame()
    {
        var gameData = new GameDataInit {
            GameName = "MyGame",
            GamePlatform = GamePlatformType.tk,
            HttpUrl = "https://api.mygame.com",
            WebSocketUrl = "wss://ws.mygame.com"
        };
        
        this.SendCommand(new GameInitCmd(gameData, null, false));
    }
    
    private void RegisterEvents()
    {
        // 监听游戏配置初始化
        this.RegisterEvent<GameConfigInitEvent>(OnGameConfigInit)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        
        // 监听 WebSocket 连接状态
        this.GetSystem<IServerCommunicationSystem>()
            .WebSocketIsConnected
            .Register(OnWebSocketConnected)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }
    
    private void OnGameConfigInit(GameConfigInitEvent e)
    {
        Debug.Log("游戏配置完成");
        
        // 获取版本号
        string version = this.SendQuery(new GameVersionQuery());
        Debug.Log($"游戏版本: {version}");
    }
    
    private void ConnectToServer()
    {
        // 连接直播服务器
        string token = GetToken(); // 从某处获取 token
        this.SendCommand(new LiveConnectCmd(token));
    }
    
    private void OnWebSocketConnected(bool isConnected)
    {
        if (isConnected)
        {
            Debug.Log("WebSocket 已连接");
            this.SendCommand<GameStartCmd>();
        }
    }
    
    private string GetToken()
    {
        // 获取 token 的逻辑
        return "your_token_here";
    }
}
```

---

## 最佳实践

1. **使用 BindableProperty**: 对于需要监听变化的数据，使用 `BindableProperty<T>`
2. **自动注销事件**: 使用 `UnRegisterWhenGameObjectDestroyed()` 避免内存泄漏
3. **命令模式**: 将业务逻辑封装在 Command 中，保持 Controller 简洁
4. **对象池**: 对频繁创建销毁的对象使用 `BetterPoolManager`
5. **异步操作**: 网络请求使用异步方法，避免阻塞主线程
6. **错误处理**: 所有网络操作都应该有错误处理逻辑
7. **日志记录**: 使用 `DebugCtrl` 统一管理日志输出

---

## 常见问题

### Q: 如何添加新的系统？
A: 创建类实现 `ISystem` 接口，继承 `AbstractSystem`，然后在 `GameArchitecture.Init()` 中注册。

### Q: Command 和 Query 的区别？
A: Command 可以修改状态，Query 只读不改变状态。

### Q: 如何在非 MonoBehaviour 类中使用架构？
A: 在 System 或 Command 中，通过 `this.GetSystem<T>()` 等方法访问。

### Q: BindableProperty 有什么优势？
A: 自动通知值变化，无需手动调用事件，代码更简洁。

### Q: 如何优化性能？
A: 使用对象池、合理配置消息处理频率、使用异步操作。

---

更多信息请参考源代码和 [架构文档](ARCHITECTURE.md)。
