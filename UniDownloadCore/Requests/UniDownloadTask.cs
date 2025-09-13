using System;
using System.Threading;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadTask
    {
        // 下载上下文接口
        private IDownloadContext _downloadContext;
        // 下载request对象
        private UniDownloadRequest _downloadRequest;
        // 分段下载层分段管理器对象
        private UniDownloadSegmentManager _segmentManager;
        // 下载任务状态
        private UniDownloadState _state;

        public Action<UniDownloadTask> OnCompleted;
        public Action<UniDownloadTask> OnCancelled;

        public UniDownloadState State
        {
            get { return _state; }
            set
            {
                switch (_state)
                {
                    case UniDownloadState.Finished:
                        OnCompleted?.Invoke(this);
                        break;
                    case UniDownloadState.Stopped:
                        OnCancelled?.Invoke(this);                        
                        break;
                    case UniDownloadState.Failure:
                        // TODO 失败的处理。任务层的失败和下载层的失败分开处理
                        break;
                }

                _state = value;
            }
        }

        public UniDownloadTask(UniDownloadRequest request)
        {
            _state = UniDownloadState.Prepare;
            _downloadRequest = request;
        }

        // 开始任务执行请求下载
        public void Start()
        {
            _state = UniDownloadState.Querying;
            _downloadContext = new UniDownloadContext(_downloadRequest.FileName);
            _downloadContext.Start(PrepareDownloadContext);
        }

        // 暂停下载，向下传递暂定处理自己的暂停逻辑
        public void Stop()
        {
            _downloadContext.Stop();
            _segmentManager.Stop();
        }

        // 准备下载上下文数据，从本地断点文件还原分段数据，如果没有从远程文件获取创建分段数据
        private void PrepareDownloadContext()
        {
            _state = UniDownloadState.Downloading;
            _segmentManager = new UniDownloadSegmentManager(_downloadContext);
            _segmentManager.Start();
        }
    }
}