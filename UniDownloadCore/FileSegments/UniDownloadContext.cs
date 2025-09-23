using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Localization.Plugins.XLIFF.V12;

namespace UniDownload.UniDownloadCore
{
    internal class UniFileInfo
    {
        public string MD5;
        public string FileName;
        public long TotalBytes;
        public long[] Donwloaded;
        public long[] StartPosition;
        public long[] EndPosition;
        public string[] SegmentName;
    }
    
    internal class UniDownloadContext : IDownloadContext, IDisposable
    {
        private const string FileInfoName = "Info";
        private string _md5;
        private int _requestId;
        private string _fileName;
        private UniFileInfo _fileInfo;
        private string _fileBaseRootPath;
        private string _fileTempRootPath;
        private string _fileInfoTempPath;
        private bool _successGetFileInfo;
        private CancellationTokenSource _cancellation;

        public int RequestId => _requestId;
        public string FileName => _fileName;
        public string FileBasePath => _fileBaseRootPath;
        public string FileTempPath => _fileTempRootPath;
        public int MaxParallel { get; private set; }
        public int Progress { get; set; }
        public long TotalBytes { get; private set; }
        public long BytesReceived { get; private set; }
        public long[] SegmentDownloaded { get; private set; }
        public long[,] SegmentRanges { get; private set; }

        public UniDownloadContext(string fileName, int requestId)
        {
            _fileName = fileName;
            _requestId = requestId;
            MaxParallel = UniUtils.GetSegmentParallel();
            _cancellation = new CancellationTokenSource(UniUtils.GetTimeOut());
            _md5 = UniUtils.GetFileNameMD5(FileName);
            _fileBaseRootPath = Path.Combine(UniUtils.GetBaseSavePath(), _md5);
            _fileTempRootPath = Path.Combine(UniUtils.GetTempSavePath(), _md5);
            _fileInfoTempPath = Path.Combine(_fileTempRootPath, FileInfoName);
        }

        // 开启装备下载上下文信息
        public void Start(Action<bool> prepareFinish)
        {
            ReadLocalInfo();
            if(_successGetFileInfo)
            {
                prepareFinish(true);
                return;
            }

            Task.Factory.StartNew(ReadRemoteFileInfoAsync, prepareFinish, _cancellation.Token,
                TaskCreationOptions.None, UniServiceContainer.Get<UniTaskScheduler>());
        }

        public void Stop()
        {
            _cancellation.Cancel();
        }

        public void Dispose()
        {
            SegmentRanges = null;
        }

        // 获取远程文件长度，并划分好分段尺寸
        private async void ReadRemoteFileInfoAsync(object state)
        {
            Action<bool> finish = state as Action<bool>;
            try
            {
                UniDownloadNetwork network = UniServiceContainer.Get<UniDownloadNetwork>();
                //获取远程头文件信息返回文件长度
                var result = await network.GetRemoteFileLength(this, _cancellation.Token);
                if (result.IsSuccess)
                {
                    TotalBytes = result.Value;
                    // 文件分段
                    var range = UniServiceContainer.Get<UniSegmentWorker>().GetSegmentRange(TotalBytes, MaxParallel);
                    // 分段之后，再次根据分段数组第一维长度，确定分段并发数
                    MaxParallel = range.Value.GetLength(0);
                    SegmentRanges = range.Value;
                    SegmentDownloaded = new long[MaxParallel];
                    finish(true);
                    return;
                }
            }
            catch (OperationCanceledException e)
            {
                UniLogger.Error($"获取远程文件被中断 文件={_fileName} 错误: {e.Message}");
            }
            catch (HttpRequestException e)
            {
                UniLogger.Error($"网络请求失败，文件名: {FileName}, 错误: {e.Message}");
            }
            catch (Exception e)
            {
                UniLogger.Error($"获取远程文件长度错误 文件={_fileName} 错误: {e.Message}");
            }

            finish(false);
        }
        
        // 从本地还原文件分段断点续传信息
        private void ReadLocalInfo()
        {
            // 如果目录或者info文件不存在表示没有断点文件
            if (!Directory.Exists(_fileTempRootPath))
            {
                return;
            }

            if (!File.Exists(_fileInfoTempPath))
            {
                return;
            }
            // 反序列文件，简单的使用Json，TODO 后续增加高性能的二进制序列化
            _fileInfo = UniUtils.DeserializeUniFileInfo(File.ReadAllBytes(_fileInfoTempPath));
            
            // 简单的判定一些基本信息确定断点文件是有效的
            _successGetFileInfo = _fileInfo.MD5 == _md5 &&
                                  _fileInfo.FileName == _fileName &&
                                  _fileInfo.TotalBytes > 0 &&
                                  _fileInfo.SegmentName != null &&
                                  _fileInfo.StartPosition != null &&
                                  _fileInfo.EndPosition != null &&
                                  _fileInfo.StartPosition.Length == _fileInfo.EndPosition.Length && 
                                  _fileInfo.StartPosition.Length == _fileInfo.SegmentName.Length;
            
            // TODO 对每个分段文件断点保存时计算md5，取出来时计算md5与保存的md5值是否一致
            if (!_successGetFileInfo)
            {
                return;
            }
            
            // 现在只通过文件是否存在，存在的文件长度与已下载量的长度一致简单判定
            for (int i = 0; i < _fileInfo.SegmentName.Length; i++)
            {
                FileInfo info = new FileInfo(Path.Combine(_fileTempRootPath, _fileInfo.SegmentName[i]));
                _successGetFileInfo = _successGetFileInfo && info.Exists && info.Length == _fileInfo.Donwloaded[i];
            }

            if (_successGetFileInfo)
            {
                // 从断点文件恢复下载总量
                TotalBytes = _fileInfo.TotalBytes;
                // 下载并发
                MaxParallel = _fileInfo.Donwloaded.Length;
                // 从断点文件恢复自己的已下载量
                SegmentDownloaded = _fileInfo.Donwloaded;
                // 从断点文件恢复下载Range
                SegmentRanges = new long[_fileInfo.EndPosition.Length, 2];
                for (long i = 0; i < _fileInfo.StartPosition.Length; i++)
                {
                    // 从断点文件恢复已下载的总量
                    BytesReceived += _fileInfo.Donwloaded[i];
                    SegmentRanges[i, 0] = _fileInfo.StartPosition[i];
                    SegmentRanges[i, 1] = _fileInfo.EndPosition[i];
                }
            }
        }
    }
}