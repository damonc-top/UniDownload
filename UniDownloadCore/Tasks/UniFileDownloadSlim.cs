using System;

namespace UniDownload
{
    /*
        增加轻量级下载任务结构，应对瞬时爆发性请求下载
    */
    internal class UniFileDownloadSlim
    {
        public string FileName;

        public Action<bool> DownloadFinish;

        public Action<int> DownloadProcess;

        public long Timestamp;
        
        public UniFileDownloadSlim(string fileName, Action<bool> downloadFinish, Action<int> downloadProcess)
        {
            FileName = fileName;
            DownloadFinish = downloadFinish;
            DownloadProcess = downloadProcess;
            Timestamp = UniUtils.TimeTicks();
        }

        public void UpdateTime()
        {
            Timestamp = UniUtils.TimeTicks();   
        }
    }
}