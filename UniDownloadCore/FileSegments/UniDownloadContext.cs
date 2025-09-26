using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    internal class UniSegmentFile
    {
        public static readonly Regex SegmentRegex = new Regex(@"^(?<fileName>.+)\.seg(?<index>\d+)\.(?<startPos>\d+)-(?<endPos>\d+)\.tmp$", RegexOptions.Compiled);
        public string SegName;  //下载文件名称
        public int SegIndex;    //分段索引
        public long TotalBytes; //下载文件总量
        public long Donwloaded; //已下载总量，
        public long StartRange; //开始范围
        public long EndRange;   //结束范围
        public bool IsDone;     //下载完成，已下载总量=endRange-startRange


        public long GetStartRange()
        {
            return Donwloaded != 0 ? Donwloaded : StartRange;
        }
        
        public static string GenSegFileName(string fileName, int index, long startRange, long endRange)
        {
            return $"{fileName}.seg{index}.{startRange}-{endRange}.tmp";
        }

        // 必须匹配格式
        // 示例：{fileName}.seg{index}.{startPos}-{endPos}.tmp
        public static bool IsMatchPattern(string segName)
        {
            return SegmentRegex.IsMatch(segName);
        }
    }
    
    internal class UniDownloadContext : IDownloadContext, IDisposable
    {
        private string _md5;
        // 整个文件请求ID
        private int _requestId;
        // 最大并发
        private int _maxParallel;
        // 下载的文件名
        private string _fileName;
        // 文件正式保存全路径
        private string _filePath;
        // 文件临时保存全路径
        private string _fileTempPath;
        // 文件临时保存的目录
        private string _fileTempRootPath;
        // 分段文件信息
        private UniSegmentFile[] _segmentFiles;
        // 取消令牌
        private CancellationTokenSource _cancellation;

        public int RequestId => _requestId;
        public string FileName => _fileName;
        
        // 文件名的md5
        public string MD5Hash => _md5;
        public string FilePath => _filePath;//正式资源全路径
        public string FileTempPath => _fileTempPath;//临时资源全路径
        public string FileTempRootPath => _fileTempRootPath;
        public int MaxParallel => _maxParallel;
        public int Progress { get; set; }
        public long TotalBytes { get; private set; }
        public long BytesReceived { get; private set; }
        public long[,] SegmentRanges { get; private set; }
        public UniSegmentFile[] SegmentFiles => _segmentFiles;

        public UniDownloadContext(string fileName, int requestId, string md5)
        {
            _fileName = fileName;
            _requestId = requestId;
            _maxParallel = UniUtils.GetSegmentParallel();
            _md5 = md5 ?? UniUtils.GetFileNameMD5(FileName);
            _filePath = Path.Combine(UniUtils.GetBaseSavePath(), FileName);
            _fileTempPath = Path.Combine(UniUtils.GetTempSavePath(), FileName);
            _fileTempRootPath = Path.Combine(UniUtils.GetTempSavePath(), _md5);
            _cancellation = new CancellationTokenSource(UniUtils.GetTimeOut());
        }

        // 开启准备下载上下文信息
        public void Start(Action<bool> prepareFinish)
        {
            // 确保正式目录存在
            UniUtils.EnsureDirectoryExists(_filePath);
            // 确保临时目录存在，后面对目录不再做判断存在
            UniUtils.EnsureDirectoryExists(_fileTempRootPath);
            // 线确保断点文件恢复成功，即可不用连接header size
            if(RecoverySegmentInfo())
            {
                prepareFinish(true);
                return;
            }

            Task.Factory.StartNew(ReadRemoteFileInfoAsync, prepareFinish, _cancellation.Token, TaskCreationOptions.None, UniServiceContainer.Get<UniTaskScheduler>());
        }

        public void StopAsync()
        {
            // 停止下载 写入断点信息
            _cancellation.Cancel();
        }

        public void Dispose()
        {
            _cancellation.Dispose();
            Directory.Delete(_fileTempRootPath, true);
        }

        // 获取远程文件长度，并划分好分段尺寸
        private async Task ReadRemoteFileInfoAsync(object state)
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
                    UniSegmentWorker segWorker = UniServiceContainer.Get<UniSegmentWorker>();
                    // 文件分段
                    var range = segWorker.GetSegmentRange(TotalBytes, MaxParallel);
                    if (range.IsSuccess)
                    {
                        // 分段之后，再次根据分段数组第一维长度，确定分段并发数
                        _maxParallel = range.Value.GetLength(0);
                        SegmentRanges = range.Value;
                        _segmentFiles = new UniSegmentFile[_maxParallel];
                        for (int i = 0; i < _segmentFiles.Length; i++)
                        {
                            _segmentFiles[i] = new UniSegmentFile()
                            {
                                SegName = UniSegmentFile.GenSegFileName(_fileName, i, SegmentRanges[i,0], SegmentRanges[i,1]),
                                SegIndex = i,
                                Donwloaded = 0,
                                IsDone = false,
                                StartRange = range.Value[i,0],
                                EndRange = range.Value[i,1],
                                TotalBytes = SegmentRanges[i,1] - SegmentRanges[i,0]
                            };
                        }
                        finish(true);
                    }
                    return;
                }
                // 失败情况已经包含在Result中，记录详细信息
                UniLogger.Error($"获取远程文件信息失败: {result.Message}");
                finish(false);
            }
            catch (Exception e)
            {
                // 只需要兜底处理意外异常
                UniLogger.Error($"获取远程文件信息发生意外异常: {_fileName}, {e.Message}");
                finish(false);
            }
        }
        
        // 从本地还原文件分段断点续传信息
        // 分段文件命名格式：{fileName}.seg{index}.{startPos}-{endPos}.tmp
        // 示例：test.bundle.seg0.0-1048575.tmp
        //      test.bundle.seg1.1048576-2097151.tmp
        // return: 成功还原到分段断点信息
        private bool RecoverySegmentInfo()
        {
            // 获取所有分段断点文件
            string[] files = Directory.GetFiles(_fileTempRootPath, $"{_fileName}.seg*.tmp");
            if (files.Length == 0) return false;
            // 提取分段文件名称，并校验文件名格式，格式错误涉嫌人为篡改直接重下整个文件
            string[] segNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                segNames[i] = files[i].Substring(_fileTempRootPath.Length);
                if (UniSegmentFile.IsMatchPattern(segNames[i])) return false;
            }

            long allBytes = 0;
            int unFinishNum = 0;
            // 恢复文件信息
            List<UniSegmentFile> segmentFiles = new List<UniSegmentFile>(segNames.Length);
            for (int i = 0; i < files.Length; i++)
            {
                string[] value = segNames[i].Split('.');
                string[] range = value[2].Split('-'); //{startPos}-{endPos}
                long startRange = long.Parse(range[0]);
                long endRange = long.Parse(range[1]);
                long totalBytes = endRange - startRange;
                allBytes += totalBytes;
                FileInfo fileInfo = new FileInfo(files[i]);
                long downloaded = fileInfo.Length;
                if (downloaded > totalBytes)
                {
                    return false;
                }

                UniSegmentFile segmentFile = new UniSegmentFile()
                {
                    SegName = value[0], //{fileName}
                    SegIndex = value[1][3], //分段索引,seg{index}
                    TotalBytes = totalBytes,
                    StartRange = startRange,
                    EndRange = endRange,
                    Donwloaded = downloaded,
                    IsDone = downloaded == totalBytes
                };
                if (!segmentFile.IsDone)
                {
                    unFinishNum++;
                }
                segmentFiles.Add(segmentFile);
            }
            
            TotalBytes = allBytes;
            _segmentFiles = segmentFiles.ToArray();
            SegmentRanges = new long[unFinishNum, 2];
            for (int i = 0; i < unFinishNum; i++)
            {
                SegmentRanges[i, 0] = segmentFiles[i].StartRange;
                SegmentRanges[i, 1] = segmentFiles[i].EndRange;
            }
            
            return true;
        }
    }
}