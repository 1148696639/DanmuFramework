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
        ///     ���һ����ʱ�����ķ���
        /// </summary>
        /// <param Name="seconds"></param>
        /// <param Name="onDelayFinish"></param>
        void AddDelayTask(float seconds, Action onDelayFinish);

        /// <summary>
        ///     ���һ�����������ķ�������Ҫ����ж�����
        /// </summary>
        /// <param Name="seconds"></param>
        /// <param Name="task"></param>
        /// <param Name="conditions"></param>
        /// <param Name="firstRun"></param>
        ///  <param Name="onFinish">����ʱ����</param>
        void AddConstantTask(float seconds, Action task, Func<bool> conditions, bool firstRun=true, Action onFinish=null);
    }

    public class DelayTask
    {
        /// <summary>
        ///     �ӳ�ʱ��
        /// </summary>
        public float Seconds { get; set; }

        /// <summary>
        ///     ִ������
        /// </summary>
        public Action OnFinish { get; set; }

        /// <summary>
        ///     ��¼��ʼʱ��
        /// </summary>
        public float StartSeconds { get; set; }

        /// <summary>
        ///     ��¼����ʱ��
        /// </summary>
        public float FinishSeconds { get; set; }

        /// <summary>
        ///     ����״̬
        /// </summary>
        public DelayTaskState State { get; set; }
    }

    public class ContainTask
    {
        /// <summary>
        ///     ���ʱ��
        /// </summary>
        public float Seconds { get; set; }

        /// <summary>
        ///     ִ������
        /// </summary>
        public Action Task { get; set; }

        /// <summary>
        ///     �ж�����
        /// </summary>
        public Func<bool> Conditions { get; set; }

        /// <summary>
        ///     ���ڼ�ʱ
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        ///     Ϊtrue��ִ��
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