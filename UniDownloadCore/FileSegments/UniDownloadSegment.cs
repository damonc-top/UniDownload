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
        private string _fileName;
        private UniSegmentFile _fileSegment;
        private Stream _readStream;
        private Stream _writeStream;
        private bool _isDownloading;
        private CancellationToken _token;
        private UniTaskScheduler _scheduler;
        
        public UniDownloadSegment(string fileName, UniSegmentFile fileSegment)
        {
            _fileName = fileName;
            _fileSegment = fileSegment;
            _isDownloading = false;
            BuffSize = UniUtils.GetSegmentBuffSize();
            Buffer = new byte[BuffSize];
            _scheduler = UniServiceContainer.Get<UniTaskScheduler>();
        }

        public void Start(CancellationToken token)
        {
            _isDownloading = true;
            _token = token;
            Task.Factory.StartNew(DownloadSegments, token, TaskCreationOptions.None, _scheduler);
        }

        // 下载分段文件数据
        private async void DownloadSegments()
        {
            try
            {
                if (_fileSegment.IsDone)
                {
                    OnDownloadCompleted();
                    return;
                }
                
                UniDownloadNetwork network = UniServiceContainer.Get<UniDownloadNetwork>();
                long startRange = _fileSegment.GetStartRange();
                long endRange = _fileSegment.EndRange;
                var result = await network.GetResponseStream(_fileName, startRange, endRange, _token);
                if (!result.IsSuccess)
                {
                    UniLogger.Error(result.Message);
                    OnDownloadFailed($"下载文件{_fileName}第{_fileSegment.SegIndex}段网络异常失败");
                    return;
                }

                _readStream = result.Value;
                StartRead();
            }
            catch (OperationCanceledException e)
            {
                OnDownloadFailed($"下载文件{_fileName}第{_fileSegment.SegIndex}段被取消异常，error：{e.Message}");
            }
            catch (Exception e)
            {
                OnDownloadFailed($"下载文件{_fileName}第{_fileSegment.SegIndex}段其他异常，error：{e.Message}");
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
                OnDownloadFailed($"{_fileName} {_fileSegment.SegIndex} 读取流失败, error: {e.Message}");
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

                _fileSegment.Donwloaded += bytesRead;
                // 开始写入
                _writeStream.BeginWrite(Buffer, 0, bytesRead, OnWriteComplete, null);
            }
            catch (Exception e)
            {
                _isDownloading = false;
                OnDownloadFailed($"{_fileName} {_fileSegment.SegIndex} 读取流失败, error: {e.Message}");
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
                OnDownloadFailed($"{_fileName} {_fileSegment.SegIndex} 写入流失败, error: {e.Message}");
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
            UniDownloadEventBus.RaiseSegmentCompleted(_fileSegment.SegIndex, null);
        }

        private void OnDownloadFailed(string errorMessage)
        {
            UniDownloadEventBus.RaiseSegmentCompleted(_fileSegment.SegIndex, errorMessage);
        }

        public void Dispose()
        {
            _writeStream.Close();
            _readStream.Flush();
            _readStream.Close();
            _isDownloading = false;
            _scheduler = null;
            Buffer = null;
        }
    }
}