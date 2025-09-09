using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniDownload
{
    internal class UniMainThread : IDisposable
    {
        private bool _dispose;
        private object _lock;
        private Queue<Action> _mainThreadAction;
        
        public UniMainThread()
        {
            _dispose = false;
            _lock = new object();
            _mainThreadAction = new Queue<Action>();
        }

        public void Enqueue(Action action)
        {
            if (_dispose)
            {
                return;
            }
            
            lock (_lock)
            {
                _mainThreadAction.Enqueue(action);
            }
        }

        public void Dispose()
        {
            _dispose = true;
            _lock = null;
            _mainThreadAction = null;
        }

        public void Update()
        {
            if (_dispose)
            {
                return;
            }
            lock (_lock)
            {
                while (_mainThreadAction.Count > 0)
                {
                    _mainThreadAction.Dequeue().Invoke();             
                }
            }
        }
    }
}