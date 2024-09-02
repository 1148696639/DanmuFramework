using System;
using System.Collections.Generic;
using UnityEngine;

namespace BetterPoolManager
{
    [Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        public int initialSize = 10;
        public string poolName => prefab.name;
    }

    public class BetterPoolManager : MonoBehaviour
    {
        private static readonly Dictionary<string, ObjectPool> pools = new();

        private static List<PoolConfig> poolConfigs;


        [SerializeField]
        private List<PoolConfig> PoolConfigs; // 在 Inspector 窗口中可视化对象池列表

        private void Awake()
        {
            // 在游戏启动时初始化所有对象池
            InitializePools();
            PoolConfigs = poolConfigs;
        }

        public static void SetInit(List<PoolConfig> poolConfigList)
        {
            poolConfigs = poolConfigList;
        }


        // 初始化所有对象池
        private void InitializePools()
        {
            foreach (var config in poolConfigs)
                if (config != null && config.prefab != null)
                    CreatePool(config);
        }

        // 创建单个对象池
        private void CreatePool(PoolConfig config)
        {
            if (!pools.ContainsKey(config.poolName))
            {
                var poolRoot = new GameObject(config.poolName + " Pool");
                poolRoot.transform.SetParent(transform);

                var pool = poolRoot.AddComponent<ObjectPool>();
                pool.Initialize(config.prefab, config.initialSize);
                pools[config.poolName] = pool;
            }
        }

        // 生成对象
        public static GameObject Spawn(string poolName, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (pools.TryGetValue(poolName, out var pool)) return pool.Spawn(position, rotation, parent);

            Debug.LogWarning($"对象池中未找到对象：{poolName}");
            return null;
        }

        // 生成对象
        public static GameObject Spawn(string poolName, Transform parent)
        {
            if (pools.TryGetValue(poolName, out var pool)) return pool.Spawn(parent.position, parent.rotation, parent);

            Debug.LogWarning($"对象池中未找到对象：{poolName}");
            return null;
        }

        public static GameObject Spawn(GameObject obj, Transform parent)
        {
            var poolName = obj.name;
            if (pools.TryGetValue(poolName, out var pool)) return pool.Spawn(parent.position, parent.rotation, parent);

            Debug.LogWarning($"对象池中未找到对象：{poolName}");
            return null;
        }

        // 回收对象
        public static void Despawn(string poolName, GameObject obj)
        {
            if (pools.TryGetValue(poolName, out var pool))
                pool.Despawn(obj);
            else
                Debug.LogWarning($"对象池中未找到对象：{poolName}");
        }

        // 回收对象
        public static void Despawn(GameObject obj)
        {
            var poolName = obj.name.Split('(')[0];
            if (pools.TryGetValue(poolName, out var pool))
                pool.Despawn(obj);
            else
                Debug.LogWarning($"对象池中未找到对象：{poolName}");
        }

        // 获取对象池
        public static ObjectPool GetPool(string poolName)
        {
            return pools.GetValueOrDefault(poolName);
        }
    }
}