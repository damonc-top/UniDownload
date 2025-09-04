using System;
using UniDownload.Schedulers;

namespace UniDownload
{
    /*
        下载管理器
    */
    public class UniDownloadManager : IDisposable
    {
        private readonly IDownloadScheduler _downloadScheduler;

        public void Update(int deltaTime)
        {
            _downloadScheduler.Update(deltaTime);
        }

        public void Dispose()
        {
        }
    }
}