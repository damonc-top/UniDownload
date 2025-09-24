using System;
using System.Threading;

namespace UniDownload.UniDownloadCore
{
    internal interface IDownloadContext
    {
        public int RequestId { get; }
        public string FileName { get; }
        public string MD5Hash { get; }
        public string FilePath { get; }
        public string FileTempPath { get; }
        public int MaxParallel { get; }
        public long TotalBytes { get; }
        public long BytesReceived { get;}
        public int Progress { get; }
        public long[] SegmentDownloaded { get;}
        public long[,] SegmentRanges { get; }

        public void Start(Action<bool> prepareFinish);
        public void StopAsync();
    }
}