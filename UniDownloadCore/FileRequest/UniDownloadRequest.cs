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
    public class UniDownloadRequest
    {
        private readonly int _invalidID = -1;
        private bool _finishFlag;
        private int _progressNum;

        private HashSet<int> _lightRequest;
        private ConcurrentDictionary<int, UniDownloadSubscribe> _callbackRequest;

        public UniDownloadRequest()
        {
            _lightRequest = new HashSet<int>();
            _callbackRequest = new ConcurrentDictionary<int, UniDownloadSubscribe>();
        } 

        public int Subscribe(Action<bool> onFinish, Action<int> onProgress, object owner = null)
        {
            int id = UniID.ID;
            if (onFinish != null || onProgress != null)
            {
                UniDownloadSubscribe subscribe = new UniDownloadSubscribe(id, owner)
                {
                    OnFinish = onFinish, OnProgress = onProgress
                };
                _callbackRequest.TryAdd(id, subscribe);
                return id; 
            }

            _lightRequest.Add(id);
            return id;
        }

        public bool Unsubscribe(int requestId)
        {
            if (requestId == _invalidID)
            {
                return true;
            }
            
            if (_lightRequest.Remove(requestId))
            {
                return true;
            }

            if (_callbackRequest.TryRemove(requestId, out _))
            {
                return true;
            }
            UniLogger.Error("移除下载出错， id不正确");
            return false;
        }

        public int UnsubscribeByOwner(object owner)
        {
            var toRemove = _callbackRequest.Where(kvp => kvp.Value.Model == owner).ToList();
        
            foreach (var item in toRemove) {
                _callbackRequest.TryRemove(item.Key, out _);
            }
            
            return toRemove.Count;
        }

        public void MainThreadFinish()
        {
            foreach (var subscribe in _callbackRequest)
            {
                if (subscribe.Value.HasCallbacks)
                {
                    subscribe.Value.OnFinish.Invoke(_finishFlag);
                }
            }
        }

        public void MainThreadProgress()
        {
            foreach (var subscribe in _callbackRequest)
            {
                if (subscribe.Value.HasCallbacks)
                {
                    subscribe.Value.OnProgress.Invoke(_progressNum);
                }
            }
        }

        public void OnFinish(bool finish)
        {
            _finishFlag = finish;
            UniUtils.RegisterMainThreadEvent(MainThreadFinish);
        }

        public void OnProgress(int progress)
        {
            Interlocked.Exchange(ref _progressNum, progress);
            UniUtils.RegisterMainThreadEvent(MainThreadProgress);
        }
    }
}