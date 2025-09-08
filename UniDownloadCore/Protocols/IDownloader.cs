using System;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload
{
    /// <summary>
    /// 下载器接口
    /// </summary>
    interface IDownloader : IDisposable
    {
        /// <summary>
        /// 异步下载文件
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>下载是否成功</returns>
        Task<bool> DownloadAsync(UniDownloadFileInfo fileInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置下载速度限制
        /// </summary>
        /// <param name="bytesPerSecond">每秒字节数，0表示不限制</param>
        void SetSpeedLimit(int bytesPerSecond);

        /// <summary>
        /// 暂停下载
        /// </summary>
        void Pause();

        /// <summary>
        /// 恢复下载
        /// </summary>
        void Resume();

        /// <summary>
        /// 下载进度事件（参数：已下载字节数，总字节数）
        /// </summary>
        event Action<long, long> OnProgressChanged;

        /// <summary>
        /// 下载速度事件（参数：当前速度 bytes/sec）
        /// </summary>
        event Action<double> OnSpeedChanged;

        /// <summary>
        /// 下载完成事件（参数：是否成功）
        /// </summary>
        event Action<bool> OnDownloadCompleted;
    }
}
