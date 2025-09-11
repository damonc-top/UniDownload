namespace UniDownload.UniDownloadCore
{
    internal enum UniDownloadState
    {
        Prepare,
        Analyzing,
        Querying,
        Downloading,
        Finished,
        Stopped,
        Failure
    }
}