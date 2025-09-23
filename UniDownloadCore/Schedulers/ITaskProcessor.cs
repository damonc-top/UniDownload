using System;

namespace UniDownload.UniDownloadCore
{
    internal interface ITaskProcessor
    {
        void ProcessRequest(string fileName, int requestId);
        bool CanAcceptRequest();
    }
}