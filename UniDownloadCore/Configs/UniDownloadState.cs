namespace UniDownload.UniDownloadCore
{
    internal enum UniDownloadState
    {
        Prepare,
        Querying,
        Downloading,
        Finished,
        Stopped,
        Failure
    }
}