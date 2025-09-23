using System;
using System.Threading;

namespace UniDownload.UniDownloadCore
{
    internal interface IDownloadContext
    {
        public int RequestId { get; }
        public string FileName { get; }
        public string FileBasePath { get; }
        public string FileTempPath { get; }
        public int MaxParallel { get; }
        public long TotalBytes { get; }
        public long BytesReceived { get;}
        public int Progress { get; }
        public long[] SegmentDownloaded { get;}
        public long[,] SegmentRanges { get; }

        public void Start(Action<bool> prepareFinish);
        public void Stop();
    }
}