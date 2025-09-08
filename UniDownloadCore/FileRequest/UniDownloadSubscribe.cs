using System;

namespace UniDownload
{
    internal class UniDownloadSubscribe : IPoolable, IDisposable
    {
        private object _model;
        private bool _disposed;
        private int _subscribeId;
        private Action<bool> _onFinish;
        private Action<int> _onProgress;

        public int SubscribeId => _subscribeId;
        public object Model => _model;

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
        public UniDownloadSubscribe()
        {
        }

        // 传统构造函数（保持向后兼容）
        public UniDownloadSubscribe(int id, object model = null)
        {
            _subscribeId = id;
            _model = model;
        }
        
        /// <summary>
        /// 初始化订阅信息（替代构造函数）
        /// </summary>
        public void Initialize(int id, object model = null)
        {
            _subscribeId = id;
            _model = model;
        }
        
        public bool HasCallbacks => _onFinish != null || _onProgress != null;

        public void OnRentFromPool()
        {
            // TODO 从池中租用时的初始化（如果需要的话）
            _disposed = false;
        }

        public void OnReturnToPool()
        {
            Clear();
        }

        private void Clear()
        {
            _subscribeId = 0;
            _model = null;
            _onFinish = null;
            _onProgress = null;
        }

        public void Dispose()
        {
            _disposed = true;
            Clear();
        }
    }
}