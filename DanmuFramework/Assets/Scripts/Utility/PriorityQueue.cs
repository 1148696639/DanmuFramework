using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PriorityQueue<T>
{
    private SortedDictionary<int, Queue<T>> _priorityQueue;

    public PriorityQueue()
    {
        _priorityQueue = new SortedDictionary<int, Queue<T>>();
    }

    public void Enqueue(T item, int priority = 0)
    {
        if (!_priorityQueue.ContainsKey(priority))
        {
            _priorityQueue[priority] = new Queue<T>();
        }

        _priorityQueue[priority].Enqueue(item);
    }

    public T Dequeue()
    {
        if (_priorityQueue.Count == 0)
        {
            throw new InvalidOperationException("The priority queue is empty.");
        }

        // 获取优先级最高的队列
        var highestPriority = _priorityQueue.Keys.Max();
        var queue = _priorityQueue[highestPriority];

        // 从该队列中取出元素
        var item = queue.Dequeue();

        // 如果该队列空了，从字典中移除它
        if (queue.Count == 0)
        {
            _priorityQueue.Remove(highestPriority);
        }

        return item;
    }

    /// <summary>
    ///  队列是否为空
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return _priorityQueue.Count == 0;
    }

    /// <summary>
    ///   队列中元素的数量
    /// </summary>
    public int Count => _priorityQueue.Count;
}