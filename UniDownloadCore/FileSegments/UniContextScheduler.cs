using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    public class UniContextScheduler : TaskScheduler
    {
        private Thread _contextThread;
        
        protected override IEnumerable<Task>? GetScheduledTasks()
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