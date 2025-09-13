using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UniDownload.UniDownloadCore
{
    // 自定义的统一调度器，处理任何类型的Task，动态分配线程资源
    internal class UniTaskScheduler : TaskScheduler
    {
        private int _maxParallel;
        
        private Thread[] _segmentThreads;
        private BlockingCollection<Task> _tasks;
        private CancellationTokenSource _cancellation;

        public UniTaskScheduler()
        {
            _maxParallel = UniUtils.GetMaxParallel();
            _tasks = new BlockingCollection<Task>();
        }

        // 启动调度器内部线程运行
        public void Start()
        {
            if (_cancellation != null)
            {
                _cancellation.Dispose();
            }
            _cancellation = new CancellationTokenSource();
            for (int i = 0; i < _maxParallel; i++)
            {
                Thread worker = new Thread(DoSegmentWorker)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal,
                };
                _segmentThreads[i] = worker;
                worker.Start();
            }
        }

        // 停止调度器
        public void Stop()
        {
            _cancellation?.Cancel();
            for (int i = 0; i < _maxParallel; i++)
            {
                _segmentThreads[i].Join();
            }
        }

        public void Dispose()
        {
            Stop();
            _tasks = null;
            _cancellation.Dispose();;
            _segmentThreads = null;
            _cancellation = null;
        }
        
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks;
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new System.NotImplementedException();
        }

        private void DoSegmentWorker()
        {
            foreach (var task in _tasks.GetConsumingEnumerable(_cancellation.Token))
            {
                
            }
        }
    }
}