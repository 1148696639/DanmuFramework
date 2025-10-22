# DanmuFramework 快速入门指南

## 安装和导入

### 方式一：使用 Unity Package
1. 下载 `DMFramework.unitypackage` 文件
2. 在 Unity 中选择 **Assets > Import Package > Custom Package**
3. 选择下载的 `.unitypackage` 文件
4. 导入所有资源

### 方式二：从源码导入
1. 克隆仓库：
   ```bash
   git clone https://github.com/1148696639/DanmuFramework.git
   ```
2. 复制 `DanmuFramework/Assets/DMFramwork` 文件夹到你的 Unity 项目的 `Assets` 目录

## 环境要求

- **Unity 版本**: Unity 2021.3 或更高版本
- **.NET 版本**: .NET Standard 2.1
- **依赖库**:
  - DOTween (已包含)
  - Newtonsoft.Json (已包含)
  - 腾讯云 COSXML SDK (已包含)

## 五分钟快速开始

### 第一步：创建游戏管理器

在场景中创建一个空物体，命名为 `GameManager`，添加 `GameManager` 脚本。

```csharp
using DMFramework;
using UnityEngine;

public class MyGameManager : MonoBehaviour
{
    void Start()
    {
        // 挂载框架提供的 GameManager 脚本
        // 或者按照下面的方式配置
    }
}
```

### 第二步：配置游戏参数

在 Inspector 中配置 `GameManager` 组件：

#### 游戏配置 (GameData)
- **Game Name**: 你的游戏名称，如 "MyGame"
- **Game Platform**: 选择直播平台 (tk-抖音 / x7-小七 / ks-快手)
- **Http Url**: HTTP 服务器地址，如 "https://api.yourdomain.com"
- **Web Socket Url**: WebSocket 服务器地址，如 "wss://ws.yourdomain.com"
- **Game Exit Is Register**: 是否在游戏退出时通知服务器

#### 测试配置 (Is Test = true 时使用)
- **Is Test**: 勾选以启用测试模式
- **Test Init Data**:
  - **Key**: 主播唯一标识
  - **Room Id**: 测试房间 ID
  - **Http Url Test**: 测试环境 HTTP 地址
  - **Web Socket Url Test**: 测试环境 WebSocket 地址
  - **Web Message Handle Frequency**: 消息处理频率（0 表示每帧处理）

### 第三步：创建登录界面

创建一个新场景作为登录场景，添加以下 UI 元素：

1. **Login Button**: 登录按钮
2. **Version Text**: 版本号显示
3. **Resolution Dropdown**: 分辨率选择下拉框

然后添加 `Login` 脚本并关联 UI 元素：

```csharp
public class Login : AbstractController
{
    [Header("登录按钮")] 
    public Button LoginBtn;
    
    [Header("版本号文本")] 
    public Text VersionTxt;
    
    [Header("分辨率选择栏")] 
    public Dropdown ResolutionDrd;
    
    // 其他代码由框架提供
}
```

### 第四步：处理弹幕消息

创建一个脚本来处理收到的弹幕消息：

```csharp
using DMFramework;
using QFramework;
using UnityEngine;

public class DanmuHandler : AbstractController
{
    private void Awake()
    {
        // 注册 WebSocket 消息接收事件
        // 这里需要根据你的实际消息格式来处理
        RegisterDanmuEvents();
    }
    
    private void RegisterDanmuEvents()
    {
        // 示例：处理弹幕消息
        // 实际的事件类型需要根据你的系统定义
    }
    
    private void OnDanmuReceived(string message)
    {
        Debug.Log($"收到弹幕: {message}");
        
        // 解析弹幕内容
        // 触发游戏逻辑
        // 例如：生成游戏对象、播放特效等
    }
}
```

### 第五步：运行游戏

1. 确保已配置好服务器地址
2. 如果是测试模式，确保填写了测试数据
3. 点击 Unity 的 Play 按钮
4. 在登录界面选择分辨率，点击登录
5. 等待连接成功后，游戏开始

## 基础示例

### 示例 1：创建一个简单的控制器

```csharp
using QFramework;
using DMFramework;
using UnityEngine;

public class PlayerController : AbstractController
{
    private void Start()
    {
        // 获取游戏配置
        var config = this.GetModel<IGameConfigModel>();
        Debug.Log($"游戏名称: {config.GameName}");
        
        // 获取版本号
        string version = this.SendQuery(new GameVersionQuery());
        Debug.Log($"版本: {version}");
    }
    
    private void Update()
    {
        // 游戏逻辑
    }
}
```

### 示例 2：创建一个命令

```csharp
using QFramework;
using DMFramework;

public class SpawnEnemyCmd : AbstractCommand
{
    private readonly Vector3 position;
    
    public SpawnEnemyCmd(Vector3 pos)
    {
        position = pos;
    }
    
    protected override void OnExecute()
    {
        // 从对象池获取敌人
        // 设置位置
        // 初始化敌人数据
        
        // 发送事件通知
        this.SendEvent(new EnemySpawnedEvent { Position = position });
    }
}

// 使用命令
public void SpawnEnemy()
{
    this.SendCommand(new SpawnEnemyCmd(spawnPoint.position));
}
```

### 示例 3：使用对象池

```csharp
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    
    void Start()
    {
        // 预热对象池
        BetterPoolManager.Instance.Prewarm(bulletPrefab, 50);
    }
    
    void Fire()
    {
        // 从池中获取子弹
        GameObject bullet = BetterPoolManager.Instance.Get(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );
        
        // 3秒后归还到池
        StartCoroutine(ReturnBullet(bullet, 3f));
    }
    
    IEnumerator ReturnBullet(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        BetterPoolManager.Instance.Return(bullet);
    }
}
```

### 示例 4：监听数据变化

```csharp
using QFramework;
using DMFramework;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionStatus : AbstractController
{
    public Text statusText;
    
    private void Awake()
    {
        // 监听 WebSocket 连接状态
        this.GetSystem<IServerCommunicationSystem>()
            .WebSocketIsConnected
            .Register(OnConnectionChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }
    
    private void OnConnectionChanged(bool isConnected)
    {
        statusText.text = isConnected ? "已连接" : "未连接";
        statusText.color = isConnected ? Color.green : Color.red;
    }
}
```

### 示例 5：播放音效

```csharp
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioClip bgmClip;
    public AudioClip clickSound;
    
    void Start()
    {
        // 播放背景音乐
        AudioManager.Instance.PlayBGM(bgmClip, 0.7f, true);
    }
    
    public void OnButtonClick()
    {
        // 播放点击音效
        AudioManager.Instance.PlaySFX(clickSound);
    }
}
```

## 常见问题解决

### Q1: 导入 Package 后报错
**A**: 检查 Unity 版本是否满足要求（2021.3+），确保 .NET 版本设置为 .NET Standard 2.1。

### Q2: WebSocket 连接失败
**A**: 
- 检查服务器地址是否正确
- 确认服务器是否在运行
- 检查防火墙设置
- 在测试模式下，确保 Token 和 RoomId 正确

### Q3: 对象池不工作
**A**: 
- 确保预制体已正确配置
- 检查是否调用了 `Prewarm()` 方法
- 确认归还对象时使用的是同一个预制体引用

### Q4: 事件没有触发
**A**: 
- 检查是否正确注册了事件监听
- 确认事件类型是否匹配
- 查看是否过早注销了事件

### Q5: 获取不到 System 或 Model
**A**: 
- 确保已在 `GameArchitecture.Init()` 中注册
- 检查是否在架构初始化之前调用

## 调试技巧

### 启用详细日志

```csharp
// 在游戏开始时设置
DebugCtrl.EnableVerboseLogging = true;

// 输出调试信息
DebugCtrl.Log("游戏已启动");
DebugCtrl.LogWarning("这是一个警告");
DebugCtrl.LogError("这是一个错误");
```

### 性能监控

```csharp
void Start()
{
    // 开始性能测试
    PerformanceTester.Instance.StartTest(60);
}

void OnGUI()
{
    // 显示 FPS
    float fps = PerformanceTester.Instance.GetCurrentFPS();
    GUI.Label(new Rect(10, 10, 100, 20), $"FPS: {fps:F1}");
}
```

### 测试 Token 获取

在命令行启动游戏时传入 Token：

```bash
# 抖音平台
YourGame.exe -token=your_token_here

# 快手平台
YourGame.exe -c your_token_here
```

## 项目结构建议

```
Assets/
├── DMFramwork/              # 框架核心文件（不要修改）
├── Scripts/
│   ├── Controllers/         # 游戏控制器
│   ├── Commands/            # 自定义命令
│   ├── Models/              # 自定义模型
│   ├── Systems/             # 自定义系统
│   └── Utilities/           # 工具类
├── Prefabs/                 # 预制体
├── Scenes/                  # 场景
│   ├── Login.unity          # 登录场景
│   └── MainGame.unity       # 主游戏场景
├── Resources/               # 资源文件
└── StreamingAssets/         # 流式资源
```

## 下一步

1. 阅读 [架构文档](ARCHITECTURE.md) 了解框架设计
2. 查看 [API 文档](API.md) 学习详细 API 使用
3. 研究 `test.cs` 中的示例代码
4. 根据你的需求扩展框架功能

## 获取帮助

- **GitHub Issues**: https://github.com/1148696639/DanmuFramework/issues
- **Email**: 1148696639@qq.com

## 许可证

请查看项目许可证文件。

---

祝你开发愉快！🎮
