
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Localization.Plugins.XLIFF.V12;

namespace UniDownload.UniDownloadCore
{
    // 任务层，维护Task对象，并发开启任务对象下载，从这里锁定最大并发数防止请求层爆发式推送
    // 任务层，调度开启任务，任务下载的终止只支持内部优先级级调度，不与request终止绑定，也就是用户可以取消request回调但是任务层根据任务优先级调
    // 度来决定是继续下载还是终止下载(终止一个必定开启新的一个)。
    // TODO 任务优先级调度，eg,正在下载4个高优先级资源，这时用户请求下载紧急资源，就会终止下载某个静默资源任务《终止和调度策略》。
    // TODO 由于请求层与任务层进行了隔离，只有请求层知道紧急请求到达，请求层对任务层频繁推送紧急任务的《接收策略》，或者推送策略。
    internal class UniDownloadTaskScheduler : ITaskProcessor
    {
        private int _maxParallel;
        private Task _longTask;
        private SemaphoreSlim _semaphoreSlim;
        private CancellationTokenSource _cancellationTokenSource;
        // 所有任务的队列，需要排序
        private BlockingCollection<UniDownloadTask> _taskQueue;

        public event Action<int> OnFinish;
        
        public UniDownloadTaskScheduler()
        {
            _maxParallel = 4;
            _semaphoreSlim = new SemaphoreSlim(_maxParallel);
            _taskQueue = new BlockingCollection<UniDownloadTask>();
        }
        
        public void ProcessRequest(string fileName, int requestId)
        {
            var task = new UniDownloadTask(fileName, requestId)
            {
                OnCompleted = OnTaskCompleted,
                OnCancelled = OnTaskCanceled
            };
            _taskQueue.Add(task);
        }

        public bool CanAcceptRequest()
        {
            return _taskQueue.Count < _maxParallel;
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

        public void Dispose()
        {
            _semaphoreSlim.Dispose();
        }

        private void LongRunningTask()
        {
            foreach (var task in _taskQueue.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                _semaphoreSlim.Wait(_cancellationTokenSource.Token);
                task.Start();
            }
        }
        
        private void OnTaskCompleted(UniDownloadTask task)
        {
            OnFinish(task.RequestId);
            _semaphoreSlim.Release();
        }

        private void OnTaskCanceled(UniDownloadTask task)
        {
            _semaphoreSlim.Release();
        }
    }
}