using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Transform
{
    private readonly Transform parent;
    private readonly List<T> pool; // 管理对象的列表
    private readonly T prefab;

    public ObjectPool(T prefab, int initialSize, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new List<T>();

        for (var i = 0; i < initialSize; i++) CreateNewObject();
    }

    private T CreateNewObject()
    {
        var newObj = Object.Instantiate(prefab, parent);
        newObj.gameObject.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }

    public Transform GetObject()
    {
        Transform obj=null;
        var index = 0;
        if (pool.Count > 0)
        {
            Transform res = null;
            while (!res)
            {
                if (index >= pool.Count)
                {
                    res = CreateNewObject();
                }
                else
                {
                    if (pool[index].gameObject.activeSelf)
                        index++;
                    else
                        res = pool[index];
                }
                obj = res;
            }
        }
        else
        {
            obj = CreateNewObject();
        }

        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReleaseObject(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Add(obj); // 将对象添加到列表的末尾
    }

    public void ReleaseAllObjects()
    {
        foreach (var obj in pool) obj.gameObject.SetActive(false);
    }

    public void ClearPool()
    {
        foreach (var obj in pool) Object.Destroy(obj.gameObject);
        pool.Clear();
    }
}