
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

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
        private ConcurrentDictionary<int, UniDownloadTask> _downloadingTasks;
        
        public UniDownloadTaskScheduler()
        {
            _maxParallel = 4;
            _semaphoreSlim = new SemaphoreSlim(_maxParallel);
            _taskQueue = new BlockingCollection<UniDownloadTask>();
            _downloadingTasks = new ConcurrentDictionary<int, UniDownloadTask>();
        }
        
        public void ProcessRequest(string fileName, int requestId)
        {
            var task = new UniDownloadTask(fileName, requestId);
            _taskQueue.Add(task);
        }

        public bool CanAcceptRequest()
        {
            return _taskQueue.Count < _maxParallel;
        }

        public void Start()
        {
            UniDownloadEventBus.DownloadTaskCompleted += OnTaskFinish;
            _cancellationTokenSource = new CancellationTokenSource();
            _longTask = new Task(LongRunningTask, TaskCreationOptions.LongRunning);
            _longTask.Start();
        }
        
        // 主线程调用
        public void Update()
        {

        }

        public void Stop()
        {
            UniDownloadEventBus.DownloadTaskCompleted -= OnTaskFinish;
            _cancellationTokenSource.Dispose();
        }

        public void Dispose()
        {
            _semaphoreSlim.Dispose();
        }

        private void LongRunningTask()
        {
            foreach (var task in _taskQueue.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                if (_downloadingTasks.TryAdd(task.RequestId, task))
                {
                    _semaphoreSlim.Wait(_cancellationTokenSource.Token);
                    task.Start();    
                }
                else
                {
                    UniLogger.Error("添加下载失败");
                }
            }
        }

        private void OnTaskFinish(object sender, UniDownloadEventArgs args)
        {
            _semaphoreSlim.Release();
            if (!_downloadingTasks.TryRemove(args.RequestId, out UniDownloadTask task))
            {
                UniLogger.Error($"下载完成回调没有找到处于下载队列的task {args.RequestId}");
                return;
            }

            if (args.ErrorMessage != null)
            {
                OnTaskFailed(task, args.ErrorMessage);
                return;
            }

            task.OnTaskCompleted();
        }

        private void OnTaskFailed(UniDownloadTask task, string errorMessage)
        {
            UniLogger.Error(errorMessage);
            task.OnTaskFailed();
        }
    }
}