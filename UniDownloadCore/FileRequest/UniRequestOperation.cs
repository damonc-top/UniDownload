using System;

namespace UniDownload
{
    internal class UniRequestOperation : IPoolable, IDisposable
    {
        private bool _disposed;
        private int _uuid;
        private Action<bool> _onFinish;
        private Action<int> _onProgress;

        public int UUID => _uuid;

        public Action<bool> OnFinish
        {
            get => _onFinish;
            set => _onFinish = value;
        }
        
        public Action<int> OnProgress
        {
            get => _onProgress;
            set => _onProgress = value;
        }

        // 无参构造函数（对象池需要）
        public UniRequestOperation()
        {
            _uuid = UniUUID.ID;
        }
        
        public bool HasCallbacks => _onFinish != null || _onProgress != null;

        public void OnRentFromPool()
        {
            // TODO 从池中租用时的初始化（如果需要的话）
            Clear();
        }

        public void OnReturnToPool()
        {
            Clear();
        }

        private void Clear()
        {
            _disposed = false;
            _uuid = -1;
            _onFinish = null;
            _onProgress = null;
        }

        public void Dispose()
        {
            Clear();
            _disposed = true;
        }
    }
}