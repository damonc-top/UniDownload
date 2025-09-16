
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
        // 正在下载中的任务，最大max parallel个
        private ConcurrentDictionary<int, UniDownloadTask> _downloading;
        // 所有任务的保活队列，不再重新对同一个下载文件实例化Task
        private ConcurrentDictionary<string, UniDownloadTask> _keepAliving;

        public UniDownloadTaskScheduler()
        {
            _maxParallel = 4;
            _semaphoreSlim = new SemaphoreSlim(_maxParallel);
            _taskQueue = new BlockingCollection<UniDownloadTask>();
            _downloading = new ConcurrentDictionary<int, UniDownloadTask>();
            _keepAliving = new ConcurrentDictionary<string, UniDownloadTask>();
        }
        
        public void ProcessRequest(UniDownloadRequest request)
        {
            if (_keepAliving.TryGetValue(request.FileName, out _))
            {
                UniLogger.Log("有下载任务正在保活队列，不再重新实例化，等待它二次启动即可");
                return;
            }
            var task = new UniDownloadTask(request.FileName, request.RequestId)
            {
                OnCompleted = OnTaskCompleted,
                OnCancelled = OnTaskCanceled
            };
            _taskQueue.Add(task);
            _keepAliving.TryAdd(request.FileName, task);
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

        public void StopTask(int requestId)
        {
            if (_downloading.TryRemove(requestId, out var task))
            {
                task.Stop();
                // 把task停止后，回收到任务队列
                Task.Factory.StartNew(ReAddTask, task, _cancellationTokenSource.Token, TaskCreationOptions.None,
                    UniServiceContainer.Get<UniTaskScheduler>());
            }
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
                if (_downloading.TryAdd(task.Uuid, task))
                {
                    task.Start();    
                }
                else
                {
                    // TODO 如果这里_downloading.TryAdd异常失败，整体调度逻辑有严重异常，说明被重复添加重复下载
                    UniLogger.Error("尝试开启下载任务时异常：添加到下载中队列失败");
                    Thread.Sleep(1000);
                    _taskQueue.Add(task);
                }
            }
        }
        
        private void OnTaskCompleted(UniDownloadTask task)
        {
            _keepAliving.TryRemove(task.FileName, out _);
            _semaphoreSlim.Release();
        }

        private void OnTaskCanceled(UniDownloadTask task)
        {
            _keepAliving.TryRemove(task.FileName, out _);
            _semaphoreSlim.Release();
        }

        // 任务被回收
        private void ReAddTask(object state)
        {
            // 延迟回收
            Thread.Sleep(1000);
            if (_cancellationTokenSource.IsCancellationRequested) return;
            var task = state as UniDownloadTask;
            _taskQueue.Add(task);
        }
    }
}