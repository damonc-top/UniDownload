using System;
using System.Threading;

namespace UniDownload.UniDownloadCore
{
    // 用户接口，开启、下载任务
    internal class UniDownloadTask
    {
        private string _md5 = null;
        // 下载任务状态
        private UniDownloadState _state;
        // 下载上下文接口
        private IDownloadContext _downloadContext;
        // 分段下载层分段管理器对象
        private UniDownloadSegmentManager _segmentManager;

        public int RequestId { get; private set; }
        public string FileName { get; private set; }

        public UniDownloadState State
        {
            get { return _state; }
            set
            {
                switch (_state)
                {
                    case UniDownloadState.Finished:
                        // TODO 合并文件
                        break;
                    case UniDownloadState.Stopped:
                    case UniDownloadState.Failure:
                        // TODO 写入分段信息
                        break;
                }

                _state = value;
            }
        }

        public UniDownloadTask(string fileName, int requestId)
        {
            Initialize(fileName, requestId);
        }

        public UniDownloadTask(string fileName, int requestId, string md5)
        {
            _md5 = md5;
            Initialize(fileName, requestId);
        }

        private void Initialize(string fileName, int requestId)
        {
            _state = UniDownloadState.Prepare;
            FileName = fileName;
            RequestId = requestId;
        }

        // 开始任务执行请求下载
        public void Start()
        {
            _state = UniDownloadState.Querying;
            _downloadContext = new UniDownloadContext(FileName, RequestId, _md5);
            _downloadContext.Start(PrepareDownloadContext);
        }

        // 暂停下载，向下传递暂定处理自己的暂停逻辑
        public void StopAsync()
        {
            _state = UniDownloadState.Stopped;
            _downloadContext?.StopAsync();
            _segmentManager?.StopAsync();
        }

        // 准备下载上下文数据，从本地断点文件还原分段数据，如果没有从远程文件获取创建分段数据
        private void PrepareDownloadContext(bool isPrepared)
        {
            if (!isPrepared)
            {
                // 准备上下文数据失败，远程head size获取失败了
                OnTaskFailed();
                return;
            }
            _state = UniDownloadState.Downloading;
            _segmentManager = new UniDownloadSegmentManager(_downloadContext);
            _segmentManager.Start();
        }

        public void OnTaskCompleted()
        {
            _state = UniDownloadState.Finished;
        }

        public void OnTaskStopped()
        {
            _state = UniDownloadState.Stopped;
        }

        public void OnTaskFailed()
        {
            _state = UniDownloadState.Failure;
        }
    }
}