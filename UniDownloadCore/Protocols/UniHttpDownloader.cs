using System;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload
{
    internal class UniHttpDownloader : IDownloader
    {
        public UniHttpDownloader()
        {
            
        }
        
        public UniHttpDownloader(HttpDownloadConfig config)
        {
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DownloadAsync(UniDownloadFileInfo fileInfo, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void SetSpeedLimit(int bytesPerSecond)
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public event Action<long, long> OnProgressChanged;
        public event Action<double> OnSpeedChanged;
        public event Action<bool> OnDownloadCompleted;
    }
}