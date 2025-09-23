using System;
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
            UniDownloadEventBus.SegmentCompleted += OnSegmentComplete;
            _cancellation = new CancellationTokenSource();
            StartDownloadSegment();
        }

        // 停止任务下载
        public void Stop()
        {
            UniDownloadEventBus.SegmentCompleted -= OnSegmentComplete;
            _cancellation?.Cancel();
            _cancellation?.Dispose();
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
                _fileSegmentThread[i].Start(writeStreams[i], _downloadContext.SegmentDownloaded[i],
                    _downloadContext.SegmentRanges[i, 1], _cancellation.Token);
            }
        }

        private void OnSegmentComplete(object sender, UniDownloadEventArgs args)
        {
            if (args.RequestId != _downloadContext.RequestId) return;
            if (args.ErrorMessage != null)
            {
                OnSegmentFailed(args.RequestId, args.ErrorMessage);
                return;
            }
            
            // TODO 成功发送
            UniDownloadEventBus.RaiseDownloadTaskCompleted(_downloadContext.RequestId, _downloadContext.FileName, null);
        }

        private void OnSegmentFailed(int segmentIndex, string errorMessage)
        {
            UniLogger.Error(errorMessage);
            
            // TODO 失败次数达到上限抛出错误回调
            UniDownloadEventBus.RaiseDownloadTaskCompleted(
                _downloadContext.RequestId, _downloadContext.FileName, errorMessage);
        }
    }
}