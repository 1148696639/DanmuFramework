# DanmuFramework 开发指南

## 目录

1. [扩展开发](#扩展开发)
2. [最佳实践](#最佳实践)
3. [设计模式应用](#设计模式应用)
4. [性能优化](#性能优化)
5. [安全建议](#安全建议)
6. [测试策略](#测试策略)

---

## 扩展开发

### 添加新的 System

System 层负责业务逻辑，是框架的核心层。

#### 步骤 1: 定义接口

```csharp
using QFramework;

namespace DMFramework
{
    public interface IInventorySystem : ISystem
    {
        // 定义系统能力
        void AddItem(string itemId, int count);
        void RemoveItem(string itemId, int count);
        int GetItemCount(string itemId);
        BindableProperty<int> TotalItems { get; }
    }
}
```

#### 步骤 2: 实现 System

```csharp
using System.Collections.Generic;
using QFramework;

namespace DMFramework
{
    public class InventorySystem : AbstractSystem, IInventorySystem
    {
        private Dictionary<string, int> items = new Dictionary<string, int>();
        
        public BindableProperty<int> TotalItems { get; } = new BindableProperty<int>(0);
        
        protected override void OnInit()
        {
            // 初始化逻辑
            DebugCtrl.Log("InventorySystem 已初始化");
        }
        
        public void AddItem(string itemId, int count)
        {
            if (items.ContainsKey(itemId))
                items[itemId] += count;
            else
                items[itemId] = count;
                
            UpdateTotalItems();
            this.SendEvent(new ItemAddedEvent { ItemId = itemId, Count = count });
        }
        
        public void RemoveItem(string itemId, int count)
        {
            if (!items.ContainsKey(itemId)) return;
            
            items[itemId] = Mathf.Max(0, items[itemId] - count);
            UpdateTotalItems();
            this.SendEvent(new ItemRemovedEvent { ItemId = itemId, Count = count });
        }
        
        public int GetItemCount(string itemId)
        {
            return items.ContainsKey(itemId) ? items[itemId] : 0;
        }
        
        private void UpdateTotalItems()
        {
            int total = 0;
            foreach (var count in items.Values)
                total += count;
            TotalItems.Value = total;
        }
    }
}
```

#### 步骤 3: 注册到架构

```csharp
public class GameArchitecture : Architecture<GameArchitecture>
{
    protected override void Init()
    {
        // 注册已有的系统...
        
        // 注册新系统
        RegisterSystem<IInventorySystem>(new InventorySystem());
    }
}
```

#### 步骤 4: 使用 System

```csharp
public class InventoryUI : AbstractController
{
    private void Start()
    {
        var inventory = this.GetSystem<IInventorySystem>();
        
        // 监听物品总数变化
        inventory.TotalItems.Register(OnTotalItemsChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }
    
    public void OnPickupItem(string itemId)
    {
        this.GetSystem<IInventorySystem>().AddItem(itemId, 1);
    }
    
    private void OnTotalItemsChanged(int total)
    {
        Debug.Log($"物品总数: {total}");
    }
}
```

---

### 添加新的 Model

Model 层负责数据存储和管理。

#### 步骤 1: 定义接口

```csharp
public interface IPlayerModel : IModel
{
    string PlayerName { get; set; }
    int Level { get; set; }
    int Experience { get; set; }
    BindableProperty<int> Gold { get; set; }
}
```

#### 步骤 2: 实现 Model

```csharp
public class PlayerModel : AbstractModel, IPlayerModel
{
    public string PlayerName { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public BindableProperty<int> Gold { get; set; } = new BindableProperty<int>(0);
    
    protected override void OnInit()
    {
        // 从 PlayerPrefs 加载数据
        LoadFromPlayerPrefs();
    }
    
    private void LoadFromPlayerPrefs()
    {
        PlayerName = PlayerPrefs.GetString("PlayerName", "Player");
        Level = PlayerPrefs.GetInt("Level", 1);
        Experience = PlayerPrefs.GetInt("Experience", 0);
        Gold.Value = PlayerPrefs.GetInt("Gold", 0);
    }
    
    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.SetInt("Level", Level);
        PlayerPrefs.SetInt("Experience", Experience);
        PlayerPrefs.SetInt("Gold", Gold.Value);
        PlayerPrefs.Save();
    }
}
```

#### 步骤 3: 注册并使用

```csharp
// 注册
RegisterModel<IPlayerModel>(new PlayerModel());

// 使用
var player = this.GetModel<IPlayerModel>();
player.Gold.Value += 100;
```

---

### 添加新的 Command

Command 封装一次完整的业务操作。

#### 完整的命令示例

```csharp
public class LevelUpCmd : AbstractCommand
{
    protected override void OnExecute()
    {
        var player = this.GetModel<IPlayerModel>();
        var inventory = this.GetSystem<IInventorySystem>();
        
        // 检查经验值
        int requiredExp = player.Level * 100;
        if (player.Experience < requiredExp)
        {
            DebugCtrl.LogWarning("经验值不足，无法升级");
            return;
        }
        
        // 升级
        player.Level++;
        player.Experience -= requiredExp;
        
        // 奖励金币
        player.Gold.Value += player.Level * 10;
        
        // 奖励物品
        inventory.AddItem("level_up_gift", 1);
        
        // 发送事件
        this.SendEvent(new PlayerLevelUpEvent 
        { 
            NewLevel = player.Level 
        });
        
        // 保存数据
        player.SaveToPlayerPrefs();
        
        DebugCtrl.Log($"升级成功！当前等级: {player.Level}");
    }
}
```

---

### 添加新的 Query

Query 用于只读查询，不改变状态。

```csharp
public class CanLevelUpQuery : AbstractQuery<bool>
{
    protected override bool OnDo()
    {
        var player = this.GetModel<IPlayerModel>();
        int requiredExp = player.Level * 100;
        return player.Experience >= requiredExp;
    }
}

// 使用
bool canLevelUp = this.SendQuery(new CanLevelUpQuery());
if (canLevelUp)
{
    this.SendCommand<LevelUpCmd>();
}
```

---

### 添加新的 Event

#### 定义事件

```csharp
public struct PlayerLevelUpEvent
{
    public int NewLevel;
}

public struct ItemAddedEvent
{
    public string ItemId;
    public int Count;
}

public struct ItemRemovedEvent
{
    public string ItemId;
    public int Count;
}
```

#### 发送和监听

```csharp
// 发送事件
this.SendEvent(new PlayerLevelUpEvent { NewLevel = 5 });

// 监听事件
this.RegisterEvent<PlayerLevelUpEvent>(OnPlayerLevelUp)
    .UnRegisterWhenGameObjectDestroyed(gameObject);

private void OnPlayerLevelUp(PlayerLevelUpEvent e)
{
    Debug.Log($"玩家升级到 {e.NewLevel} 级！");
    ShowLevelUpEffect();
}
```

---

## 最佳实践

### 1. 代码组织

#### 推荐的文件结构

```
Scripts/
├── Game/
│   ├── GameArchitecture.cs      # 架构入口
│   └── GameConfig.cs             # 游戏配置
├── Models/
│   ├── IPlayerModel.cs
│   └── PlayerModel.cs
├── Systems/
│   ├── IInventorySystem.cs
│   └── InventorySystem.cs
├── Commands/
│   ├── LevelUpCmd.cs
│   └── SaveGameCmd.cs
├── Queries/
│   ├── CanLevelUpQuery.cs
│   └── GetPlayerStatsQuery.cs
├── Events/
│   └── GameEvents.cs             # 所有事件定义
├── Controllers/
│   ├── PlayerController.cs
│   └── UIController.cs
└── Utilities/
    └── GameHelper.cs
```

### 2. 命名规范

```csharp
// Interface: I + 名称 + 层级
IPlayerModel
IInventorySystem

// Implementation: 名称 + 层级
PlayerModel
InventorySystem

// Command: 动作 + Cmd
LevelUpCmd
SaveGameCmd

// Query: 描述 + Query
CanLevelUpQuery
GetPlayerStatsQuery

// Event: 名称 + Event
PlayerLevelUpEvent
ItemAddedEvent

// BindableProperty: 清晰的属性名
IsConnected
TotalItems
PlayerHealth
```

### 3. 使用 BindableProperty

#### 什么时候使用

```csharp
// ✅ 需要监听变化的数据
public BindableProperty<int> Health { get; set; }
public BindableProperty<bool> IsAlive { get; set; }

// ❌ 不需要监听的数据
public string PlayerName { get; set; }
public int Level { get; set; }
```

#### 正确的使用方式

```csharp
// System 中定义
public class HealthSystem : AbstractSystem, IHealthSystem
{
    public BindableProperty<int> CurrentHealth { get; } = new BindableProperty<int>(100);
    
    public void TakeDamage(int damage)
    {
        CurrentHealth.Value -= damage; // 自动触发监听
        
        if (CurrentHealth.Value <= 0)
        {
            this.SendEvent<PlayerDeathEvent>();
        }
    }
}

// Controller 中监听
public class HealthBar : AbstractController
{
    private void Start()
    {
        this.GetSystem<IHealthSystem>()
            .CurrentHealth
            .Register(OnHealthChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }
    
    private void OnHealthChanged(int newHealth)
    {
        healthBar.fillAmount = newHealth / 100f;
    }
}
```

### 4. 事件注销管理

```csharp
// ✅ 推荐：自动注销
this.RegisterEvent<GameEvent>(OnGameEvent)
    .UnRegisterWhenGameObjectDestroyed(gameObject);

// ✅ 推荐：手动管理
private IUnRegister eventUnRegister;

void OnEnable()
{
    eventUnRegister = this.RegisterEvent<GameEvent>(OnGameEvent);
}

void OnDisable()
{
    eventUnRegister?.UnRegister();
}

// ❌ 不推荐：不注销（会导致内存泄漏）
this.RegisterEvent<GameEvent>(OnGameEvent);
```

### 5. 对象池使用

```csharp
public class BulletManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    
    void Start()
    {
        // 预热对象池
        BetterPoolManager.Instance.Prewarm(bulletPrefab, 50);
    }
    
    void Fire()
    {
        // 获取对象
        GameObject bullet = BetterPoolManager.Instance.Get(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );
        
        // 配置子弹
        var bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Initialize(damage, speed);
    }
    
    // Bullet 脚本中归还对象
    public class Bullet : MonoBehaviour
    {
        void OnCollisionEnter(Collision collision)
        {
            // 处理碰撞
            
            // 归还到池
            BetterPoolManager.Instance.Return(gameObject);
        }
    }
}
```

---

## 设计模式应用

### 1. 命令模式

将请求封装为对象，便于参数化、排队、撤销。

```csharp
// 可撤销的命令
public class MovePieceCmd : AbstractCommand
{
    private readonly Vector3 fromPos;
    private readonly Vector3 toPos;
    private readonly GameObject piece;
    
    public MovePieceCmd(GameObject piece, Vector3 from, Vector3 to)
    {
        this.piece = piece;
        this.fromPos = from;
        this.toPos = to;
    }
    
    protected override void OnExecute()
    {
        piece.transform.position = toPos;
    }
    
    public void Undo()
    {
        piece.transform.position = fromPos;
    }
}
```

### 2. 观察者模式

通过事件系统实现。

```csharp
// 发布者
public class GameSystem : AbstractSystem
{
    public void DoSomething()
    {
        // 业务逻辑
        
        // 发布事件
        this.SendEvent(new SomethingHappenedEvent());
    }
}

// 订阅者
public class Observer : AbstractController
{
    void Start()
    {
        this.RegisterEvent<SomethingHappenedEvent>(OnSomethingHappened);
    }
    
    void OnSomethingHappened(SomethingHappenedEvent e)
    {
        // 响应事件
    }
}
```

### 3. 单例模式

框架自动处理 System 和 Model 的单例。

```csharp
// ✅ 通过架构获取（推荐）
var system = this.GetSystem<IMySystem>();

// ❌ 不要手动实现单例
public class MySystem : MonoBehaviour
{
    public static MySystem Instance; // 不推荐
}
```

### 4. 工厂模式

通过 Factory 创建复杂对象。

```csharp
public class EnemyFactory : IUtility
{
    public GameObject CreateEnemy(EnemyType type, Vector3 position)
    {
        GameObject prefab = GetPrefab(type);
        GameObject enemy = BetterPoolManager.Instance.Get(
            prefab, position, Quaternion.identity
        );
        
        InitializeEnemy(enemy, type);
        return enemy;
    }
    
    private GameObject GetPrefab(EnemyType type)
    {
        // 根据类型返回预制体
    }
    
    private void InitializeEnemy(GameObject enemy, EnemyType type)
    {
        // 初始化敌人属性
    }
}
```

---

## 性能优化

### 1. 对象池优化

```csharp
// 在游戏开始时预热
void Start()
{
    // 预热常用对象
    BetterPoolManager.Instance.Prewarm(bulletPrefab, 100);
    BetterPoolManager.Instance.Prewarm(enemyPrefab, 50);
    BetterPoolManager.Instance.Prewarm(effectPrefab, 30);
}

// 在关卡结束时清理
void OnLevelEnd()
{
    BetterPoolManager.Instance.Clear(bulletPrefab);
}
```

### 2. 事件优化

```csharp
// ✅ 使用 struct 事件（值类型，无 GC）
public struct PlayerMoveEvent
{
    public Vector3 Position;
}

// ❌ 避免使用 class 事件
public class PlayerMoveEvent  // 会产生 GC
{
    public Vector3 Position;
}
```

### 3. BindableProperty 优化

```csharp
// 只在值真正改变时才触发
property.Value = newValue;  // 框架已优化，相同值不会触发

// 批量更新时使用 SetValueWithoutEvent
property.SetValueWithoutEvent(newValue);  // 不触发事件
// ... 更多更新
property.Value = finalValue;  // 最后触发一次
```

### 4. 消息处理优化

```csharp
// 控制消息处理频率
gameConfigModel.WebMessageHandleFrequency = 0.1f;  // 每 0.1 秒处理一次
```

---

## 安全建议

### 1. Token 安全

```csharp
// ❌ 不要硬编码 Token
private const string TOKEN = "my_secret_token";

// ✅ 从启动参数或配置文件读取
string token = GetTokenFromCommandLine();

// ✅ 在正式版本中加密存储
string encryptedToken = EncryptHelper.Encrypt(token);
```

### 2. 数据验证

```csharp
public void ProcessDanmu(string message)
{
    // ✅ 验证输入
    if (string.IsNullOrEmpty(message))
    {
        DebugCtrl.LogWarning("收到空消息");
        return;
    }
    
    if (message.Length > 1000)
    {
        DebugCtrl.LogWarning("消息过长，已忽略");
        return;
    }
    
    // 处理消息
}
```

### 3. 网络安全

```csharp
// ✅ 使用 HTTPS 和 WSS
gameConfigModel.HttpUrlBase = "https://api.example.com";
gameConfigModel.WebSocketUrl = "wss://ws.example.com";

// ✅ 处理网络错误
commSystem.PostRequestAsync(url, body, (success, response) =>
{
    if (!success)
    {
        DebugCtrl.LogError($"请求失败: {response}");
        // 重试逻辑
        return;
    }
    
    // 处理响应
});
```

---

## 测试策略

### 1. 单元测试

```csharp
using NUnit.Framework;

public class PlayerModelTests
{
    private IPlayerModel playerModel;
    
    [SetUp]
    public void Setup()
    {
        playerModel = new PlayerModel();
        playerModel.Init();
    }
    
    [Test]
    public void AddGold_IncreasesGoldAmount()
    {
        // Arrange
        int initialGold = playerModel.Gold.Value;
        
        // Act
        playerModel.Gold.Value += 100;
        
        // Assert
        Assert.AreEqual(initialGold + 100, playerModel.Gold.Value);
    }
}
```

### 2. 集成测试

```csharp
[Test]
public void LevelUpCmd_IncreasesLevel()
{
    // Arrange
    var player = Architecture.Interface.GetModel<IPlayerModel>();
    player.Experience = 100;
    int initialLevel = player.Level;
    
    // Act
    Architecture.Interface.SendCommand<LevelUpCmd>();
    
    // Assert
    Assert.AreEqual(initialLevel + 1, player.Level);
}
```

### 3. 测试模式

```csharp
// 启用测试模式
public class GameManager : AbstractController
{
    [Header("测试")]
    public bool IsTest = true;
    
    void Start()
    {
        if (IsTest)
        {
            // 使用测试数据
            InitTestMode();
        }
        else
        {
            // 正常初始化
            InitNormalMode();
        }
    }
}
```

---

## 总结

遵循这些开发指南和最佳实践，你可以：

1. ✅ 编写清晰、可维护的代码
2. ✅ 避免常见的性能问题
3. ✅ 保证代码的安全性
4. ✅ 轻松扩展框架功能
5. ✅ 提高开发效率

记住：**好的架构让开发变得简单，好的实践让代码更加优雅。**

---

更多信息请参考：
- [README.md](README.md) - 项目概述
- [ARCHITECTURE.md](ARCHITECTURE.md) - 架构详解
- [API.md](API.md) - API 文档
- [QUICKSTART.md](QUICKSTART.md) - 快速入门
