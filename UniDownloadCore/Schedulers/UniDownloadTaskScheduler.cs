
using System;
using System.Collections.Concurrent;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadTaskScheduler : ITaskProcessor
    {
        private int _maxConcurrentTasks;
        private BlockingCollection<UniDownloadTask> _downloadTasks;
        
        public void ProcessRequest(UniDownloadRequest request)
        {
            var task = new UniDownloadTask(request);
            task.OnCompleted += OnTaskCompleted;
            _downloadTasks.Add(task);
        }

        public bool CanAcceptRequest()
        {
            return _downloadTasks.Count < _maxConcurrentTasks;
        }
        
        public void Update()
        {
            
        }

        public void Stop()
        {
            
        }
        
        private void OnTaskCompleted(UniDownloadTask task)
        {
            
        }
    }
}