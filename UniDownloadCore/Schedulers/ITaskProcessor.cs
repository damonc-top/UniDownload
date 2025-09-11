namespace UniDownload.UniDownloadCore
{
    internal interface ITaskProcessor
    {
        void ProcessRequest(UniDownloadRequest request);
        bool CanAcceptRequest();
    }
}