using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UniDownload
{
    /*
        增加轻量级下载任务结构，应对瞬时爆发性请求下载
    */
    internal class UniDownloadRequest : IPoolable
    {
        private readonly int _invalidID = -1;
        private bool _finishFlag;
        private int _progressNum;

        private ConcurrentDictionary<int, UniRequestOperation> _callbackRequest;

        public string FileName;

        public UniDownloadRequest()
        {
            
        }
        
        public void Initialize(string fileName)
        {
            FileName = fileName;
            _callbackRequest = new ConcurrentDictionary<int, UniRequestOperation>();
        } 

        public int ActionRegister(Action<bool> onFinish, Action<int> onProgress)
        {
            UniRequestOperation operation = UniUtils.RentRequestOperation();
            int uuid = operation.UUID;
            operation.OnFinish = onFinish;
            operation.OnProgress = onProgress;
            _callbackRequest.TryAdd(uuid, operation);
            return operation.UUID;
        }

        public bool ActionUnRegister(int uuid)
        {
            if (uuid == _invalidID)
            {
                return true;
            }

            if (_callbackRequest.TryRemove(uuid, out _))
            {
                return true;
            }
            UniLogger.Error("移除下载出错， id不正确");
            return false;
        }

        /// <summary>
        /// 主线程调用
        /// </summary>
        private void MainThreadFinish()
        {
            foreach (var subscribe in _callbackRequest)
            {
                if (subscribe.Value.HasCallbacks)
                {
                    subscribe.Value.OnFinish.Invoke(_finishFlag);
                }
            }
            //TODO 下载完成后执行回收逻辑
        }

        /// <summary>
        /// 主线程调用
        /// </summary>
        private void MainThreadProgress()
        {
            foreach (var subscribe in _callbackRequest)
            {
                if (subscribe.Value.HasCallbacks)
                {
                    subscribe.Value.OnProgress.Invoke(_progressNum);
                }
            }
        }

        /// <summary>
        /// 下载线程回调
        /// </summary>
        /// <param name="finish"></param>
        public void OnFinish(bool finish)
        {
            _finishFlag = finish;
            UniUtils.RegisterMainThreadEvent(MainThreadFinish);
        }

        /// <summary>
        /// 下载线程回调
        /// </summary>
        /// <param name="progress"></param>
        public void OnProgress(int progress)
        {
            Interlocked.Exchange(ref _progressNum, progress);
            UniUtils.RegisterMainThreadEvent(MainThreadProgress);
        }

        public void OnRentFromPool()
        {
            throw new NotImplementedException();
        }

        public void OnReturnToPool()
        {
            throw new NotImplementedException();
        }
    }
}