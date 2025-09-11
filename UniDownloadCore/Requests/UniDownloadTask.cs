using System;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadTask
    {
        private UniDownloadSegmentManager _segmentManager;

        public event Action<UniDownloadTask> OnCompleted;
        
        public UniDownloadTask(UniDownloadRequest request){}
        
        public void Start(){}
    }
}