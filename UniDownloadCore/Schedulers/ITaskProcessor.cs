using System;

namespace UniDownload.UniDownloadCore
{
    internal interface ITaskProcessor
    {
        event Action<int> OnFinish; 
        void ProcessRequest(string fileName, int requestId);
        bool CanAcceptRequest();
    }
}