namespace UniDownload.UniDownloadCore
{
    internal struct UniDownloadProgressInfo
    {
        public int RequestId { get; }
        public int Progress { get; }
        public long BytesDownloaded { get; }
        public long TotalBytes { get; }
    
        public UniDownloadProgressInfo(int requestId, int progress, long bytesDownloaded = 0, long totalBytes = 0)
        {
            RequestId = requestId;
            Progress = progress;
            BytesDownloaded = bytesDownloaded;
            TotalBytes = totalBytes;
        }
    }
}