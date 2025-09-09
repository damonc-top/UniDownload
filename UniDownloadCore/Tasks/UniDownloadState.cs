namespace UniDownload
{
    /*
        下载状态
    */
    internal enum UniDownloadState
    {
        Prepare,            // 准备中
        Analysis,           // 分析中
        Querying,           // 查询文件长度
        Downloading,        // 下载中
        DownloadMerging,    // 合并文件
        Paused,             // 暂停
        Cancelled,          // 取消
        Completed,          // 完成
        Failed,             // 失败
    }
}