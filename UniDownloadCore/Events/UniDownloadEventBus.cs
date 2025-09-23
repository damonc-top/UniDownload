using System;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadEventBus
    {
        public static event EventHandler<UniDownloadEventArgs> DownloadStarted;
        public static event EventHandler<UniDownloadProgressInfo> DownloadProgress;
        public static event EventHandler<UniDownloadEventArgs> DownloadCompleted;
        public static event EventHandler<UniDownloadEventArgs> DownloadRequestCompleted;
        public static event EventHandler<UniDownloadEventArgs> DownloadTaskCompleted;
        public static event EventHandler<UniDownloadEventArgs> SegmentCompleted;

        // 触发事件的静态方法
        public static void RaiseDownloadStarted(int requestId, string fileName)
        {
            DownloadStarted?.Invoke(null, new UniDownloadEventArgs
            {
                RequestId = requestId,
                FileName = fileName
            });
        }

        public static void RaiseDownloadProgress(int requestId, int progress, int bytesDownloaded, int totalBytes)
        {
            DownloadProgress?.Invoke(null, new UniDownloadProgressInfo(
                requestId, progress, bytesDownloaded, totalBytes
            ));
        }

        public static void RaiseDownloadCompleted(int requestId, string fileName, string errorMessage,
            Exception exception = null)
        {
            DownloadCompleted?.Invoke(null, new UniDownloadEventArgs
            {
                RequestId = requestId,
                FileName = fileName,
                ErrorMessage = errorMessage,
                Exception = exception
            });
        }
        
        public static void RaiseDownloadRequestCompleted(int requestId, string fileName, string errorMessage,
            Exception exception = null)
        {
            DownloadRequestCompleted?.Invoke(null, new UniDownloadEventArgs
            {
                RequestId = requestId,
                FileName = fileName,
                ErrorMessage = errorMessage,
                Exception = exception
            });
        }

        public static void RaiseDownloadTaskCompleted(int requestId, string fileName, string errorMessage,
            Exception exception = null)
        {
            DownloadTaskCompleted?.Invoke(null, new UniDownloadEventArgs
            {
                RequestId = requestId,
                FileName = fileName,
                ErrorMessage = errorMessage,
                Exception = exception
            });
        }

        public static void RaiseSegmentCompleted(int segmentIndex, string errorMessage)
        {
            SegmentCompleted?.Invoke(null, new UniDownloadEventArgs
            {
                // 这里用RequestId字段传递segmentIndex
                RequestId = segmentIndex,
                // errorMessage标记失败
                ErrorMessage = errorMessage
            });
        }
    }
}