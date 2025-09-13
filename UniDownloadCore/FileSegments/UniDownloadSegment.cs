using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadSegment
    {
        private int _segmentIndex;
        private Stream _readStream;
        private Stream _writeStream;

        public UniDownloadSegment(int segmentIndex, Stream writeStream)
        {
            _segmentIndex = segmentIndex;
            _writeStream = writeStream;
        }

        public void Start(CancellationToken token)
        {
            Task.Factory.StartNew(DownloadSegments, token, TaskCreationOptions.None,
                UniServiceContainer.Get<UniTaskScheduler>());
        }

        private async void DownloadSegments()
        {
            
        }
    }
}