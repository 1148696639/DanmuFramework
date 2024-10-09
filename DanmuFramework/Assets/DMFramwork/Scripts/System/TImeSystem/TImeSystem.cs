using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace DMFramework
{
    public interface ITimeSystem : ISystem
    {
        float CurrentSeconds { get; }

        /// <summary>
        ///     添加一个延时触发的方法
        /// </summary>
        /// <param Name="seconds"></param>
        /// <param Name="onDelayFinish"></param>
        void AddDelayTask(float seconds, Action onDelayFinish);

        /// <summary>
        ///     添加一个持续触发的方法，需要添加判定条件
        /// </summary>
        /// <param Name="seconds"></param>
        /// <param Name="task"></param>
        /// <param Name="conditions"></param>
        /// <param Name="firstRun"></param>
        ///  <param Name="onFinish">结束时调用</param>
        void AddConstantTask(float seconds, Action task, Func<bool> conditions, bool firstRun=true, Action onFinish=null);
    }

    public class DelayTask
    {
        /// <summary>
        ///     延迟时间
        /// </summary>
        public float Seconds { get; set; }

        /// <summary>
        ///     执行内容
        /// </summary>
        public Action OnFinish { get; set; }

        /// <summary>
        ///     记录开始时间
        /// </summary>
        public float StartSeconds { get; set; }

        /// <summary>
        ///     记录结束时间
        /// </summary>
        public float FinishSeconds { get; set; }

        /// <summary>
        ///     任务状态
        /// </summary>
        public DelayTaskState State { get; set; }
    }

    public class ContainTask
    {
        /// <summary>
        ///     间隔时间
        /// </summary>
        public float Seconds { get; set; }

        /// <summary>
        ///     执行内容
        /// </summary>
        public Action Task { get; set; }

        /// <summary>
        ///     判断条件
        /// </summary>
        public Func<bool> Conditions { get; set; }

        /// <summary>
        ///     用于计时
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        ///     为true则执行
        /// </summary>
        public bool IsRun { get; set; }

        public Action OnFinish { get; set; }
    }

    public enum DelayTaskState
    {
        NotStart,
        Started,
        Finish
    }

    public class TimeSystem : AbstractSystem, ITimeSystem
    {
        private readonly LinkedList<ContainTask> _containTasks = new();

        private readonly LinkedList<DelayTask> _delayTasks = new();

        public float CurrentSeconds { get; private set; }

        public void AddDelayTask(float seconds, Action onDelayFinish)
        {
            var delayTask = new DelayTask
            {
                Seconds = seconds,
                OnFinish = onDelayFinish,
                State = DelayTaskState.NotStart
            };

            _delayTasks.AddLast(delayTask);
        }


        public void AddConstantTask(float seconds, Action task, Func<bool> conditions, bool firstRun, Action onFinish)
        {
            var containTask = new ContainTask
            {
                Seconds = seconds,
                Conditions = conditions,
                Task = task,
                IsRun = firstRun,
                OnFinish = onFinish
            };
            _containTasks.AddLast(containTask);
        }

        protected override void OnInit()
        {
            var updateBehaviourGameObj = new GameObject(nameof(TimeSystemUpdateBehaviour));
            var updateBehaviour = updateBehaviourGameObj.AddComponent<TimeSystemUpdateBehaviour>();

            updateBehaviour.OnUpdate += OnUpdate;

            CurrentSeconds = 0;
        }

        private void OnUpdate()
        {
            CurrentSeconds += Time.deltaTime;

            if (_delayTasks.Count > 0)
            {
                var currentNode = _delayTasks.First;

                while (currentNode != null)
                {
                    var nextNode = currentNode.Next;
                    var delayTask = currentNode.Value;

                    if (delayTask.State == DelayTaskState.NotStart)
                    {
                        delayTask.State = DelayTaskState.Started;
                        delayTask.StartSeconds = CurrentSeconds;
                        delayTask.FinishSeconds = CurrentSeconds + delayTask.Seconds;
                    }
                    else if (delayTask.State == DelayTaskState.Started)
                    {
                        if (CurrentSeconds >= delayTask.FinishSeconds)
                        {
                            delayTask.State = DelayTaskState.Finish;
                            delayTask.OnFinish();

                            delayTask.OnFinish = null;

                            _delayTasks.Remove(currentNode);
                        }
                    }

                    currentNode = nextNode;
                }
            }


            if (_containTasks.Count > 0)
            {
                var currentNode = _containTasks.First;
                while (currentNode != null)
                {
                    var nextNode = currentNode.Next;
                    var containTask = currentNode.Value;
                    containTask.Time += Time.deltaTime;
                    if (containTask.IsRun)
                    {
                        if (containTask.Conditions())
                        {
                            containTask.Task();
                            containTask.IsRun = false;
                        }
                        else
                        {
                            containTask.Conditions = null;
                            containTask.Task = null;
                            containTask.OnFinish?.Invoke();
                            _containTasks.Remove(containTask);
                        }
                    }

                    if (containTask.Time >= containTask.Seconds)
                    {
                        containTask.IsRun = true;
                        containTask.Time = 0;
                    }

                    currentNode = nextNode;
                }
            }
        }

        public class TimeSystemUpdateBehaviour : MonoBehaviour
        {
            private void Awake()
            {
                DontDestroyOnLoad(gameObject);
            }

            public event Action OnUpdate;

            private void Update()
            {
                OnUpdate?.Invoke();
            }
        }
    }
}