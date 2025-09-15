using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadSegmentManager
    {
        private CancellationTokenSource _cancellation;
        private IDownloadContext _downloadContext;
        private string[] _segmentTempPaths;
        private UniDownloadSegment[] _fileSegmentThread;

        public UniDownloadSegmentManager(IDownloadContext context)
        {
            _downloadContext = context;
            CreateSegments();
        }

        // 开始任务下载
        public void Start()
        {
            _cancellation = new CancellationTokenSource();
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

            _segmentTempPaths = segmentPaths.Value;
            _fileSegmentThread = new UniDownloadSegment[_downloadContext.SegmentRanges.GetLength(0)];
            for (int i = 0; i < _segmentTempPaths.Length; i++)
            {
                _fileSegmentThread[i] = new UniDownloadSegment(i, _segmentTempPaths[i]);
            }
        }

        // 开始下载分段文件
        private void StartDownloadSegment()
        {
            UniSegmentWorker worker = UniServiceContainer.Get<UniSegmentWorker>();
            Result<Stream[]> segmentStreams = worker.GetSegmentStream(_segmentTempPaths, _downloadContext);
            if (!segmentStreams.IsSuccess)
            {
                UniLogger.Error($"文件分段错误 {_downloadContext.FileName}");
                return;
            }

            Stream[] writeStreams = segmentStreams.Value;
            for (int i = 0; i < writeStreams.Length; i++)
            {
                _fileSegmentThread[i].Start(writeStreams[i], _downloadContext, _cancellation.Token);
            }
        }
    }
}