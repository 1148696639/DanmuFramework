using System;
using UnityEngine;
using System.Collections.Generic;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void RunOnMainThread(Action action)
    {
        if (instance != null)
        {
            instance.EnqueueAction(action);
        }
    }

    private void EnqueueAction(Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }

    private readonly Queue<Action> actions = new Queue<Action>();

    private void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }
    }

    // 自动生成并挂载 UnityMainThreadDispatcher 脚本的静态方法
    public static UnityMainThreadDispatcher AutoGenerate()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("UnityMainThreadDispatcher");
            return go.AddComponent<UnityMainThreadDispatcher>();
        }
        return instance;
    }
}