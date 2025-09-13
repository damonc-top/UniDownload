using System.IO;
using System.Threading;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadSegmentManager
    {
        private CancellationTokenSource _cancellation;
        private IDownloadContext _downloadContext;
        private UniDownloadSegment[] _fileSegmentThread;

        public UniDownloadSegmentManager(IDownloadContext context)
        {
            _downloadContext = context;
        }

        // 开始任务下载
        public void Start()
        {
            _cancellation = new CancellationTokenSource();
            CreateSegments();
            StartDownloadSegment();
        }

        // 停止任务下载
        public void Stop()
        {
            _cancellation?.Cancel();
        }
        
        // 创建文件分段对象
        private void CreateSegments()
        {
            UniSegmentWorker worker = UniServiceContainer.Get<UniSegmentWorker>();
            Result<string[]> segmentPaths = worker.GetSegmentPaths(_downloadContext.MaxParallel, _downloadContext.FileTempPath);
            if (!segmentPaths.IsSuccess)
            {
                return;
            }

            string[] paths = segmentPaths.Value;
            Result<Stream[]> segmentStreams = worker.GetSegmentStream(paths, _downloadContext);
            if (!segmentStreams.IsSuccess)
            {
                UniLogger.Error($"文件分段错误 {_downloadContext.FileName}");
                return;
            }
            _fileSegmentThread = new UniDownloadSegment[_downloadContext.SegmentRanges.GetLength(0)];
            for (int i = 0; i < paths.Length; i++)
            {
                _fileSegmentThread[i] = new UniDownloadSegment(i, segmentStreams.Value[i]);
            }
        }

        // 开始现在分段文件
        private void StartDownloadSegment()
        {
            for (int i = 0; i < _fileSegmentThread.Length; i++)
            {
                _fileSegmentThread[i].Start(_cancellation.Token);
            }
        }
    }
}