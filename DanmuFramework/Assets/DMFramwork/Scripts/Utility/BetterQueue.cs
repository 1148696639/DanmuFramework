using System;
using System.Collections.Generic;
using System.Linq;
using QFramework;

public class BetterQueue<T>
{
    private readonly SortedDictionary<int, Queue<T>> _priorityQueue;

    public BetterQueue()
    {
        _priorityQueue = new SortedDictionary<int, Queue<T>>();
    }

    /// <summary>
    ///     队列中元素的数量
    /// </summary>
    public BindableProperty<int> Count { get; } = new();

    public void Enqueue(T item, int priority = 0)
    {
        if (!_priorityQueue.ContainsKey(priority)) _priorityQueue[priority] = new Queue<T>();

        _priorityQueue[priority].Enqueue(item);
        Count.Value++;
    }

    public T Dequeue()
    {
        if (_priorityQueue.Count == 0) throw new InvalidOperationException("The priority queue is empty.");

        // 获取优先级最高的队列
        var highestPriority = _priorityQueue.Keys.Max();
        var queue = _priorityQueue[highestPriority];

        // 从该队列中取出元素
        var item = queue.Dequeue();
        Count.Value--;

        // 如果该队列空了，从字典中移除它
        if (queue.Count == 0) _priorityQueue.Remove(highestPriority);

        return item;
    }

    /// <summary>
    ///     队列是否为空
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return _priorityQueue.Count == 0;
    }
}