using System;
using System.Collections.Generic;
using System.Threading;

namespace LeeFramework.Tcp
{
    public class MainThread : SynchronizationContext
    {
        public static MainThread instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new MainThread(Thread.CurrentThread.ManagedThreadId);
                }
                return _Instance;
            }
        }
        private static MainThread _Instance;
        private Queue<Action> _AllAction = new Queue<Action>();
        private int _ThreadId = 0;


        public MainThread(int id)
        {
            _ThreadId = id;
            TcpMono.instance.callback = OnUpdate;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            Post(() =>
            {
                d?.Invoke(state);
            });
        }

        public void Post(Action action)
        {
            try
            {
                if (Thread.CurrentThread.ManagedThreadId == _ThreadId)
                {
                    action?.Invoke();
                    return;
                }
                lock (_AllAction)
                {
                    _AllAction.Enqueue(action);
                }

            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// 下一阵执行
        /// </summary>
        public void Run(Action action)
        {
            lock (_AllAction)
            {
                _AllAction.Enqueue(action);
            }
        }

        public void OnUpdate()
        {
            lock (_AllAction)
            {
                if (_AllAction.Count > 0)
                {
                    while (_AllAction.Count > 0)
                    {
                        Action action = _AllAction.Dequeue();
                        action?.Invoke();
                    }
                }
            }
        }
    } 
}
