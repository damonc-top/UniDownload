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
        // 调度器
        private Task _scheduler;
        
        // 下载并发控制
        private SemaphoreSlim _downloadCtr;

        // 取消令牌
        private CancellationTokenSource _cancellation;

        // 最大并发数
        private int _maxParallel;

        // 下载队列
        private BlockingCollection<UniFileDownloadTask> _downloadQueue;
        
        // 下载ID与任务映射
        private Dictionary<int, UniDownloadRequest> _uniDownloadRequests;
        
        // 下载ID与文件名映射
        private Dictionary<string, int> _nameMapTaskIds;

        public UniDownloadScheduler()
        {
            _maxParallel = UniUtils.GetMaxParallel();
            _downloadCtr = new SemaphoreSlim(_maxParallel);
            _cancellation = new CancellationTokenSource();
            _downloadQueue = new BlockingCollection<UniFileDownloadTask>();
            _uniDownloadRequests = new Dictionary<int, UniDownloadRequest>();
            _nameMapTaskIds = new Dictionary<string, int>();
            _scheduler = new Task(LongRunning, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 主线程更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(int deltaTime)
        {

        }

        private void LongRunning()
        {
            foreach (var task in _downloadQueue.GetConsumingEnumerable(_cancellation.Token))
            {
                _downloadCtr.Wait(_cancellation.Token);
                task.Start();
            }
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
        public int AddTask(string fileName)
        {
            return AddTask(fileName, null, null);
        }

        /// <summary>
        /// 添加下载任务，同一文件不会重复下载
        /// </summary>
        /// <param name="fileName">文件名字，相对路径文件名，eg:Bundles/Android/xxx</param>
        /// <param name="finish">下载完成回调，重试次数用完或网络异常返回false</param>
        /// <param name="process">下载进度回调，进度从0-100</param>
        /// <returns></returns>
        public int AddTask(string fileName, Action<bool> finish, Action<int> process)
        {
            return 0;
        }
        
        public int AddTask(UniDownloadRequest request)
        {
            //request.SetMainThread(_uniMainThread);
            throw new NotImplementedException();
        }

        public void StopTask(int uuid)
        {
            throw new System.NotImplementedException();
        }

        public void PauseTask(int uuid)
        {
            throw new System.NotImplementedException();
        }

        public void ResumeTask(int uuid)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveTask(int uuid)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}