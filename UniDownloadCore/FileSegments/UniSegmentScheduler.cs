using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UniDownload.UniDownloadCore
{
    internal class UniSegmentScheduler : TaskScheduler
    {
        private int _maxParallel = 4;
        private Thread[] _segmentThreads;

        public UniSegmentScheduler()
        {
            for (int i = 0; i < _maxParallel; i++)
            {
                _segmentThreads[i] = new Thread(DoSegmentWorker);
            }
        }
        
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new System.NotImplementedException();
        }

        protected override void QueueTask(Task task)
        {
            throw new System.NotImplementedException();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new System.NotImplementedException();
        }

        private void DoSegmentWorker()
        {
            
        }
    }
}