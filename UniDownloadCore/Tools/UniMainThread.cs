using System;
using System.Collections.Generic;

namespace UniDownload.UniDownloadCore
{
    internal class UniMainThread : IDisposable
    {
        private object _lock;
        private Queue<Action> _actions;

        public UniMainThread()
        {
            _lock = new object();
            _actions = new Queue<Action>();
        }

        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Enqueue(action);
            }
        }

        public void Update()
        {
            lock (_lock)
            {
                while (_actions.Count > 0)
                {
                    _actions.Dequeue().Invoke();
                }
            }
        }

        public void Dispose()
        {
            _lock = null;
            _actions = null;
        }
    }
}