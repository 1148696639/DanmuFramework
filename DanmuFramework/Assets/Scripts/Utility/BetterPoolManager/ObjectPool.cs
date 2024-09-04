using System.Collections.Generic;
using UnityEngine;

namespace BetterPoolManager
{
    /// <summary>
    ///  会按照生成实例的顺序回收实例，用于UI对象
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        private readonly Queue<GameObject> availableObjects = new();
        private GameObject prefab;

        // 初始化对象池
        public void Initialize(GameObject prefab, int initialSize)
        {
            this.prefab = prefab;

            for (var i = 0; i < initialSize; i++) CreateNewObject();
        }

        // 创建新的对象并加入池中
        private GameObject CreateNewObject()
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            availableObjects.Enqueue(obj);
            return obj;
        }

        // 从池中生成对象
        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject obj;

            if (availableObjects.Count > 0)
                obj = availableObjects.Dequeue();
            else
                obj = CreateNewObject();

            obj.SetActive(true);
            obj.transform.SetParent(parent);
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        // 回收对象
        public void Despawn(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            availableObjects.Enqueue(obj);
        }

        // 清空对象池
        public void ClearPool()
        {
            while (availableObjects.Count > 0)
            {
                var obj = availableObjects.Dequeue();
                Destroy(obj);
            }
        }
    }
}