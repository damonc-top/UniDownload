using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace UniDownload.UniDownloadCore
{
    // 请求层，维护的request对象,维护用户发起下载请求、取消请求
    // 请求层，对请求进行热点排序，遴选高优先级请求推送给任务层
    internal class UniDownloadRequestScheduler
    {
        private bool _stop;
        private int _maxParallel;
        private int _maxActivating;
        private readonly object _lock;
        private readonly ITaskProcessor _taskProcessor;
        
        // 全量request对象
        private List<UniDownloadRequest> _requests;
        
        // 处于下载中的request
        private List<UniDownloadRequest> _activeRequests;
        
        // operation ID映射request
        private Dictionary<int, UniDownloadRequest> _requestActions;
        
        // file ID映射request
        private Dictionary<int, UniDownloadRequest> _requestsFinish;
        
        // file Name映射request
        private Dictionary<string, UniDownloadRequest> _requestRepeats;

        public UniDownloadRequestScheduler(ITaskProcessor processor)
        {
            _lock = new object();
            _taskProcessor = processor;
            _maxParallel = UniUtils.GetMaxParallel();
            _requests = new List<UniDownloadRequest>();
            _activeRequests = new List<UniDownloadRequest>();
            //这次创建了三个字典，主要目的是想要快速查找，避免全量遍历request列表
            _requestActions = new Dictionary<int, UniDownloadRequest>();
            _requestsFinish = new Dictionary<int, UniDownloadRequest>();
            _requestRepeats = new Dictionary<string, UniDownloadRequest>();
        }

        public void Update()
        {
            if (_stop)
            {
                return;
            }

            CheckRequestLifeTime();
            PushRequestToTask();
        }


        public int AddRequest(string fileName, bool isHighest, Action finish, Action<int> progress)
        {
            lock (_lock)
            {
                if (!_requestRepeats.TryGetValue(fileName, out var request))
                {
                    request = new UniDownloadRequest();
                    request.Initialize(fileName);
                    AddRequestToList(request);
                    _requestRepeats[fileName] = request;
                    _requestsFinish[request.RequestId] = request;
                }

                request.SetRequestMode(isHighest);
                int uuid = request.Register(finish, progress);
                _requestActions[uuid] = request;
                
                return uuid;
            }
        }

        // 取消下载请求
        public void RemoveRequest(int uuid)
        {
            lock (_lock)
            {
                if (_requestActions.TryGetValue(uuid, out var request))
                {
                    request.UnRegister(uuid);
                    _requestActions.Remove(uuid);
                    _requestsFinish.Remove(uuid);
                }
            }
        }

        public void Start()
        {
            _stop = false;
        }
        
        public void Stop()
        {
            _stop = true;
        }
        
        public void Dispose()
        {
            _requests = null;
            _requestsFinish = null;
            _requestActions = null;
            _activeRequests = null;
            _requestRepeats = null;
        }

        // 检查request的生命周期是否有效，如果被标记了取消并同时到期，就要回收该对象
        private void CheckRequestLifeTime()
        {
            int lifeTime = UniUtils.GetRequestLifeTime();
            int time = UniUtils.GetTime();
            lock (_lock)
            {
                foreach (var request in _requests)
                {
                    if (request.IsCanceling && (request.HotTime + lifeTime) <= time)
                    {
                        request.LifeTimeExpired();
                        _requests.Remove(request);
                        break;
                    }
                }
            }
        }

        // 把请求推送给任务层
        private void PushRequestToTask()
        {
            lock (_lock)
            {
                if (_requests.Count < 1 || _maxActivating >= _maxParallel)
                {
                    return;
                }

                int available = _maxParallel - _maxActivating;
                int toTakeNum = Math.Min(available, _requests.Count);
                for (int i = 0; i < toTakeNum; i++)
                {
                    _activeRequests.Add(_requests[0]);
                    _requests.RemoveAt(0);
                }
            }
            
            // 锁外安全推送
            foreach (var request in _activeRequests)
            {
                if (_taskProcessor.CanAcceptRequest())
                {
                    _taskProcessor.ProcessRequest(request);
                    Interlocked.Increment(ref _maxActivating);
                }
                else
                {
                    break;
                }
            }
            _activeRequests.Clear();
        }

        // 请求加入下载列表并排序
        private void AddRequestToList(UniDownloadRequest request)
        {
            _requests.Add(request);
            _requests.Sort(SortRequestList);
        }

        // 在添加元素时进行一次排序，只是移除时list是自动补位的不需要再一次排序
        private int SortRequestList(UniDownloadRequest a, UniDownloadRequest b)
        {
            // 比较高优先级
            int priorityComparison = b.IsHighest.CompareTo(a.IsHighest);
            if (priorityComparison != 0) return priorityComparison;
    
            // 如果优先级相同且都是高优先级，或都是 Activating 状态，比较 HotTime
            if ((a.IsHighest && b.IsHighest) || 
                (a.IsActivating && b.IsActivating))
            {
                return b.HotTime.CompareTo(a.HotTime);
            }
    
            // 比较 Activating 状态
            int stateComparison = (b.IsActivating).CompareTo(a.IsActivating);
            if (stateComparison != 0) return stateComparison;
    
            // 最后比较 HotTime
            return b.HotTime.CompareTo(a.HotTime);
        }
        
        // request下载完成时、被回收时回调，就要从维护字典移除
        private void OnFinish(UniDownloadRequest request)
        {
            lock (_lock)
            {
                _activeRequests.Remove(request);
                _requestRepeats.Remove(request.FileName);
                _maxActivating--;
            }
        }
    }
}