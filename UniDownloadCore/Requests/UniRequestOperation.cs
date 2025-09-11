using System;

namespace UniDownload.UniDownloadCore
{
    public class UniRequestOperation : IDisposable
    {
        private int _uuid;
        public Action OnFinish;
        public Action<int> OnProgress;
        
        public int UUID => _uuid;

        public UniRequestOperation()
        {
            _uuid = UniUUID.NextID;
        }

        public void Dispose()
        {
            _uuid = 0;
            OnFinish = null;
            OnProgress = null;
        }
    }
}