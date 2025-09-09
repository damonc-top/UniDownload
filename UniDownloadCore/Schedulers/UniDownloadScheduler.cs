using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload
{
    /*
        下载调度器
    */
    internal class UniDownloadScheduler : IDownloadScheduler
    {
        private int TakeDelay = 1000;
        
        // 调度器
        private Task _scheduler;
        
        // 下载并发控制
        private SemaphoreSlim _downloadCtr;

        // 取消令牌
        private CancellationTokenSource _cancellation;

        // 最大并发数
        private int _maxParallel;

        // 请求队列
        private BlockingCollection<UniDownloadRequest> _requestQueue;

        // 下载队列
        private ConcurrentDictionary<int, UniDownloadTask> _activeTask;
        
        private ConcurrentDictionary<string, UniDownloadRequest> _requestsFileMap;
        
        public UniDownloadScheduler()
        {
            _maxParallel = UniUtils.GetMaxParallel();
            _downloadCtr = new SemaphoreSlim(_maxParallel);
            _cancellation = new CancellationTokenSource();
            _requestQueue = new BlockingCollection<UniDownloadRequest>();
            _activeTask = new ConcurrentDictionary<int, UniDownloadTask>();
            _requestsFileMap = new ConcurrentDictionary<string, UniDownloadRequest>();
            _scheduler = new Task(LongRunning, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 主线程更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {

        }

        private void LongRunning()
        {
            foreach (var request in _requestQueue.GetConsumingEnumerable(_cancellation.Token))
            {
                _downloadCtr.Wait(_cancellation.Token);
                StartDownloadTask(request);
            }
        }

        private void StartDownloadTask(UniDownloadRequest request)
        {
            UniDownloadTask.Create(request.FileName)
        }
        
        private void OnRequestFinish(int uuid, UniDownloadRequest request, bool finish)
        {
            _downloadCtr.Release();
            request.OnFinish(finish);
            _requestsFileMap.TryRemove(request.FileName, out _);
        }

        private void OnRequestProgress(UniDownloadRequest request, int progress)
        {
            request.OnProgress(progress);
        }
        
        public void Start()
        {
            _scheduler.Start();
        }

        public void Stop()
        {
            _cancellation.Cancel();
        }

        /// <summary>
        /// 添加下载任务，同一文件不会重复下载
        /// </summary>
        /// <param name="fileName">文件名字，相对路径文件名，eg:Bundles/Android/xxx</param>
        /// <returns></returns>
        public int AddRequest(string fileName)
        {
            return AddRequest(fileName, null, null);
        }

        /// <summary>
        /// 添加下载任务，同一文件不会重复下载
        /// </summary>
        /// <param name="fileName">文件名字，相对路径文件名，eg:Bundles/Android/xxx</param>
        /// <param name="finish">下载完成回调，重试次数用完或网络异常返回false</param>
        /// <param name="progress">下载进度回调，进度从0-100</param>
        /// <returns></returns>
        public int AddRequest(string fileName, Action<bool> finish, Action<int> progress)
        {
            if (!_requestsFileMap.TryGetValue(fileName, out var request))
            {
                request = UniUtils.RentDownloadRequest();
                request.Initialize(fileName);
                _requestsFileMap.TryAdd(fileName, request);
            }
            return request.ActionRegister(finish, progress);
        }

        public void StopRequest(int uuid, object owner)
        {
            foreach (var request in _requestQueue)
            {
                //TODO operation的uuid映射到request，避免全量循环压力
                request.ActionUnRegister(uuid);
            }
            if (_activeTask.TryGetValue(uuid, out var downloadTask))
            {
                downloadTask.Stop();
            }
        }

        public void PauseRequest(int uuid)
        {
            throw new System.NotImplementedException();
        }

        public void ResumeRequest(int uuid)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveRequest(int uuid)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}