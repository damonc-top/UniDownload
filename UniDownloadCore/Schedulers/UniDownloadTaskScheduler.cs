
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadTaskScheduler : ITaskProcessor
    {
        private int _maxParallel;
        private Task _longTask;
        private SemaphoreSlim _semaphoreSlim;
        private CancellationTokenSource _cancellationTokenSource;
        private BlockingCollection<UniDownloadTask> _downloadTasks;

        public UniDownloadTaskScheduler()
        {
            _maxParallel = 4;
            _semaphoreSlim = new SemaphoreSlim(_maxParallel);
            _downloadTasks = new BlockingCollection<UniDownloadTask>();
        }
        
        public void ProcessRequest(UniDownloadRequest request)
        {
            var task = new UniDownloadTask(request);
            task.OnCompleted = OnTaskCompleted;
            _downloadTasks.Add(task);
        }

        public bool CanAcceptRequest()
        {
            return _downloadTasks.Count < _maxParallel;
        }

        public void Start()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                _longTask = new Task(LongRunningTask, TaskCreationOptions.LongRunning);
                _longTask.Start();
            }
        }
        
        // 主线程调用
        public void Update()
        {

        }

        public void Stop()
        {
            //_longTask.Status
            _cancellationTokenSource.Cancel();
        }

        private void LongRunningTask()
        {
            foreach (var task in _downloadTasks.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                _semaphoreSlim.Wait(_cancellationTokenSource.Token);
                task.OnCompleted = OnTaskCompleted;
                task.OnCancelled = OnTaskCanceled;
                task.Start();
            }
        }
        
        private void OnTaskCompleted(UniDownloadTask task)
        {
            _semaphoreSlim.Release();
        }

        private void OnTaskCanceled(UniDownloadTask task)
        {
            _semaphoreSlim.Release();
        }
    }
}