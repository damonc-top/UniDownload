using System;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadTask
    {
        private IDownloadContext _downloadContext;
        private UniDownloadRequest _downloadRequest;
        private UniDownloadSegmentManager _segmentManager;

        public Action<UniDownloadTask> OnCompleted;
        public Action<UniDownloadTask> OnCancelled;

        public UniDownloadTask(UniDownloadRequest request)
        {
            _downloadRequest = request;
            _downloadContext = new UniDownloadContext();
        }
        
        public void Start(){}
    }
}