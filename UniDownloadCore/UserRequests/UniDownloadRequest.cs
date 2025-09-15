using System;
using System.Collections.Generic;
using System.Threading;

namespace UniDownload.UniDownloadCore
{
    // 用户请求层
    internal class UniDownloadRequest : IDisposable
    {
        private int _fileId;
        private int _hotTime;
        private int _refCount;
        private int _progress;
        private string _fileName;
        private RequestState _state;
        private Action<int> _onRequestFinish;
        private UniRequestMode _mode;
        private Dictionary<int, UniRequestOperation> _requestOperations;

        public int HotTime => _hotTime;
        public int FileId => _fileId;
        public string FileName => _fileName;
        public RequestState State => _state;
        public bool IsHighest => _mode == UniRequestMode.ManualMode;
        public bool IsDownloading => _state == RequestState.Downloading;
        public bool IsActivating => _state == RequestState.Activating;
        public bool IsCanceling => _state == RequestState.Canceling;
        
        public UniDownloadRequest() { }

        // 初始化
        public void Initialize(string fileName, Action<int> onRequestFinish)
        {
            _refCount = 1;
            _progress = 0;
            _fileName = fileName;
            _fileId = UniUUID.NextID;
            _state = RequestState.Activating;
            _onRequestFinish = onRequestFinish;
            _requestOperations = new Dictionary<int, UniRequestOperation>();
            _hotTime = UniUtils.GetTime();
        }

        // 设置优先级
        public void SetRequestMode(bool isHighest)
        {
            _mode = isHighest ? UniRequestMode.ManualMode : UniRequestMode.SilentMode;
            _hotTime = UniUtils.GetTime();
        }

        // 注册下载回调，刷新热点时间与标记状态
        // 返回的是该次请求注册的operationId
        public int Register(Action onFinish, Action<int> onProgress)
        {
            _refCount++;
            _hotTime = UniUtils.GetTime();
            if (_state != RequestState.Downloading)
            {
                _state = RequestState.Activating;
            }
            int uuid;
            if (onFinish != null || onProgress != null)
            {
                UniRequestOperation operation = new UniRequestOperation();
                operation.OnFinish = onFinish;
                operation.OnProgress = onProgress;
                uuid = operation.Uuid;
                _requestOperations[uuid] = operation;
            }
            else
            {
                uuid = UniUUID.NextID;
            }
            return uuid;
        }

        // 取消注册下载回调，引用为0标记取消状态
        public void UnRegister(int uuid)
        {
            _refCount--;
            _requestOperations.Remove(uuid);
            if (_refCount <= 0)
            {
                _state = IsDownloading ? _state : RequestState.Canceling;
                _mode = IsDownloading ? UniRequestMode.SilentMode : _mode;
                _hotTime = UniUtils.GetTime();
            }
        }
        
        // 下载线程回调
        public void OnFinish()
        {
            UniServiceContainer.Get<UniMainThread>().Enqueue(OnMainThreadFinish);
        }
        
        // 下载线程回调
        public void OnProgress(int progress)
        {
            Interlocked.Exchange(ref _progress, progress);
            UniServiceContainer.Get<UniMainThread>().Enqueue(OnMainThreadProgress);
        }

        // 生命周期结束回收
        public void LifeTimeExpired()
        {
            // TODO 对象自身的回收逻辑
            foreach (var operation in _requestOperations)
            {
                _onRequestFinish(operation.Value.Uuid);
            }
        }
        
        public void Dispose()
        {
            _fileId = -1;
            _requestOperations = null;
            _state = RequestState.Disposed;
        }

        // 主线程回调
        private void OnMainThreadFinish()
        {
            foreach (var operation in _requestOperations)
            {
                operation.Value.OnFinish();
                _onRequestFinish(operation.Value.Uuid);
            }
        }

        // 主线程回调
        private void OnMainThreadProgress()
        {
            foreach (var operation in _requestOperations)
            {
                operation.Value.OnProgress(_progress);
            }
        }

    }
}