using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadSegment
    {
        private int BuffSize;
        private byte[] Buffer;
        private int _byteRead;
        private string _fileName;
        private string _segmentPath;
        private int _segmentIndex;
        private Stream _readStream;
        private Stream _writeStream;
        private IDownloadContext _downloadContext;
        private long _startRnage;
        private long _endRange;
        private CancellationToken _token;
        private UniTaskScheduler _scheduler;
        
        public UniDownloadSegment(int segmentIndex, string segmentPath)
        {
            BuffSize = UniUtils.GetSegmentBuffSize();
            Buffer = new byte[BuffSize];
            _scheduler = UniServiceContainer.Get<UniTaskScheduler>();
            _segmentIndex = segmentIndex;
            _segmentPath = segmentPath;
        }

        public void Start(Stream writeStream, IDownloadContext context, CancellationToken token)
        {
            _byteRead = -1;
            _token = token;
            _writeStream = writeStream;
            _downloadContext = context;
            _startRnage = context.SegmentDownloaded[_segmentIndex];
            _endRange = context.SegmentRanges[_segmentIndex, 1];
            Task.Factory.StartNew(DownloadSegments, token, TaskCreationOptions.None, _scheduler);
        }

        private async void DownloadSegments()
        {
            try
            {
                UniDownloadNetwork network = UniServiceContainer.Get<UniDownloadNetwork>();
                var result = await network.GetResponseStream(_fileName, _startRnage, _endRange, _token);
                if (!result.IsSuccess)
                {
                    UniLogger.Error(result.Message);
                    return;
                }

                _readStream = result.Value;
                DownloadFile();
            }
            catch (OperationCanceledException e)
            {
                UniLogger.Error($"下载文件{_fileName}第{_segmentIndex}段被取消异常");
            }
        }

        private async void DownloadFile()
        {
            try
            {
                while (true)
                {
                    int bytesRead = await _readStream.ReadAsync(Buffer, 0, BuffSize, _token);
                    if (bytesRead == 0) break;
                    _byteRead += bytesRead;
                    await _writeStream.WriteAsync(Buffer, 0, bytesRead, _token);
                }
            }
            catch (Exception ex)
            {
                UniLogger.Error($"下载段{_segmentIndex}出错: {ex.Message}");
            }
        }
    }
}