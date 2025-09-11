using System;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadContext : IDownloadContext, IDisposable
    {
        public int TotalBytes { get; private set; }
        public int BytesReceived { get; private set; }
        public int[][] SegmentPositions { get; private set; }

        public UniDownloadContext()
        {
            PrepareLocalInfo();
        }

        public async void PrepareRemoteInfo()
        {
            Task infoTask = Task.Factory.StartNew(PrepareRemoteInfo);
            await infoTask;
            infoTask.GetAwaiter().GetResult();
        }
        
        public void Dispose()
        {
            SegmentPositions = null;
        }
        
        // 从本地还原文件分段信息
        private void PrepareLocalInfo()
        {
            
        }
    }
}