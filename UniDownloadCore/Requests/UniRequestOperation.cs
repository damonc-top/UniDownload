using System;

namespace UniDownload.UniDownloadCore
{
    public class UniRequestOperation : IDisposable
    {
        public Action OnFinish;
        public Action<int> OnProgress;
        
        public int Uuid { get; private set; }

        public UniRequestOperation()
        {
            Uuid = UniUUID.NextID;
        }

        public void Dispose()
        {
            Uuid = 0;
            OnFinish = null;
            OnProgress = null;
        }
    }
}