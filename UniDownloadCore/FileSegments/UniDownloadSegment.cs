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
        private int _byteRecived;
        private string _fileName;
        private string _segmentPath;
        private int _segmentIndex;
        private Stream _readStream;
        private Stream _writeStream;
        private long _startRnage;
        private long _endRange;
        private bool _isDownloading;
        private CancellationToken _token;
        private UniTaskScheduler _scheduler;
        
        public UniDownloadSegment(int segmentIndex, string segmentPath)
        {
            _isDownloading = false;
            BuffSize = UniUtils.GetSegmentBuffSize();
            Buffer = new byte[BuffSize];
            _scheduler = UniServiceContainer.Get<UniTaskScheduler>();
            _segmentIndex = segmentIndex;
            _segmentPath = segmentPath;
        }

        public void Start(Stream writeStream, long startRange, long endRange, CancellationToken token)
        {
            _isDownloading = true;
            _byteRecived = 0;
            _token = token;
            _writeStream = writeStream;
            _startRnage = startRange;
            _endRange = endRange;
            Task.Factory.StartNew(DownloadSegments, token, TaskCreationOptions.None, _scheduler);
        }

        // 下载分段文件数据
        private async void DownloadSegments()
        {
            try
            {
                UniDownloadNetwork network = UniServiceContainer.Get<UniDownloadNetwork>();
                var result = await network.GetResponseStream(_fileName, _startRnage, _endRange, _token);
                if (!result.IsSuccess)
                {
                    UniLogger.Error(result.Message);
                    OnDownloadFailed($"下载文件{_fileName}第{_segmentIndex}段网络异常失败");
                    return;
                }

                _readStream = result.Value;
                StartRead();
            }
            catch (OperationCanceledException e)
            {
                OnDownloadFailed($"下载文件{_fileName}第{_segmentIndex}段被取消异常，error：{e.Message}");
            }
            catch (Exception e)
            {
                OnDownloadFailed($"下载文件{_fileName}第{_segmentIndex}段其他异常，error：{e.Message}");
            }
        }

        // 开始读取response stream流
        private void StartRead()
        {
            if(!_isDownloading || IsCancellationRequest())
            {
                return;
            }
            try
            {
                // 使用APM编程模型，不使用await和async，这里的buff空间较小，会频繁读写
                // await之后线程释放要同步上下文。IO完成之后线程要恢复上下文，就会有线程切换开销
                // 使用APM把IO操作放给IO线程(线程池)，不占用调度器线程
                _readStream.BeginRead(Buffer, 0, BuffSize, OnReadComplete, null);
            }
            catch (Exception e)
            {
                _isDownloading = false;
                OnDownloadFailed($"{_fileName} {_segmentIndex} 读取流失败, error: {e.Message}");
            }
        }
        
        private void OnReadComplete(IAsyncResult state)
        {
            if (IsCancellationRequest())
            {
                return;
            }
            try
            {
                int bytesRead = _readStream.EndRead(state);
                if (bytesRead == 0)
                {
                    // 下载完成
                    _isDownloading = false;
                    OnDownloadCompleted();
                    return;
                }

                _byteRecived += bytesRead;
                // 开始写入
                _writeStream.BeginWrite(Buffer, 0, bytesRead, OnWriteComplete, null);
            }
            catch (Exception e)
            {
                _isDownloading = false;
                OnDownloadFailed($"{_fileName} {_segmentIndex} 读取流失败, error: {e.Message}");
            }
        }

        private void OnWriteComplete(IAsyncResult state)
        {
            if (IsCancellationRequest())
            {
                return;
            }
            try
            {
                if (_token.IsCancellationRequested)
                {
                    _isDownloading = false;
                    return;
                }
                _writeStream.EndWrite(state);
                StartRead();
            }
            catch (Exception e)
            {
                _isDownloading = false;
                OnDownloadFailed($"{_fileName} {_segmentIndex} 写入流失败, error: {e.Message}");
            }
        }

        private bool IsCancellationRequest()
        {
            if (_token.IsCancellationRequested)
            {
                _isDownloading = false;
                return true;
            }
            return false;
        }

        private void OnDownloadCompleted()
        {
            _writeStream.Close();
            _readStream.Flush();
            _readStream.Close();
            UniDownloadEventBus.RaiseSegmentCompleted(_segmentIndex, null);
        }

        private void OnDownloadFailed(string errorMessage)
        {
            UniDownloadEventBus.RaiseSegmentCompleted(_segmentIndex, errorMessage);
        }
    }
}