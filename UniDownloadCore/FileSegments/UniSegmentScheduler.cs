using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace UniDownload.UniDownloadCore
{
    internal class UniSegmentScheduler : TaskScheduler
    {
        private Thread[] _maxParallelThread;
        
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
    }
}