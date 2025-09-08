using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload
{
    public class UniTaskScheduler : TaskScheduler
    {
        private Thread[] _worker;
        private SemaphoreSlim _semaphore;
        private ConcurrentQueue<Task> _highPriorityTasks;
        private CancellationTokenSource _cancellationSource;

        public UniTaskScheduler(int maxParallel)
        {
            _cancellationSource = new CancellationTokenSource();
            _worker = new Thread[maxParallel];
            _semaphore = new SemaphoreSlim(maxParallel);
            for (int i = 0; i < maxParallel; i++)
            {
                _worker[i] = new Thread(DoThreadWorker)
                {
                    IsBackground = true,
                    Name = $"UniDownload-Task-Worker-{i}"
                };
            }
        }

        public void Start()
        {
            for (int i = 0; i < _worker.Length; i++)
            {
                _worker[i].Start();
            }
        }

        public void Stop()
        {
            _cancellationSource.Cancel();
            for (int i = 0; i < _worker.Length; i++)
            {
                _worker[i].Join();
            }
        }

        public void Dispose()
        {
            Stop();
            _worker = null;
            _semaphore = null;
            _highPriorityTasks = null;
            _cancellationSource = null;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _highPriorityTasks;
        }

        protected override void QueueTask(Task task)
        {
            throw new System.NotImplementedException();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new System.NotImplementedException();
        }

        private void DoThreadWorker()
        {
            CancellationToken token = _cancellationSource.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _semaphore.Wait(token);
                    //TryExecuteTask(task)
                }
            }
            catch (OperationCanceledException e)
            {
                UniLogger.Error(e.Message);
            }
            catch (Exception e)
            {
                UniLogger.Error(e.Message);
            }
        }
    }
}