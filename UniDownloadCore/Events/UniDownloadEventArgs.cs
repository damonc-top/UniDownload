using System;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadEventArgs : EventArgs
    {
        public int RequestId { get; set; }
        public string FileName { get; set; }
        public int Progress { get; set; }
        public string FilePath { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
    }
}