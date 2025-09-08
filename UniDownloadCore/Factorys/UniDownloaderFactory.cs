using System;

namespace UniDownload
{
    /// <summary>
    /// 下载器工厂实现
    /// </summary>
    internal class UniDownloaderFactory : IDownloaderFactory
    {
        public IDownloader Create(NetworkSpeed networkSpeed, NetworkType networkType)
        {
            // 智能选择策略：在你的场景下，HTTP总是最佳选择
            // 未来如果需要可以根据网络状况选择不同配置
            
            switch (networkSpeed)
            {
                case NetworkSpeed.Slow:
                    // 慢速网络：使用HTTP，配置小分片
                    return CreateHttpDownloader(
                        chunkSize: 512 * 1024,      // 512KB
                        maxConcurrent: 2,           // 限制并发
                        connectionTimeout: 30000,   // 30秒超时
                        retryCount: 5               // 多次重试
                    );
                    
                case NetworkSpeed.Medium:
                    return CreateHttpDownloader(
                        chunkSize: 2 * 1024 * 1024, // 2MB
                        maxConcurrent: 4,
                        connectionTimeout: 20000,
                        retryCount: 3
                    );
                    
                case NetworkSpeed.Fast:
                    return CreateHttpDownloader(
                        chunkSize: 8 * 1024 * 1024, // 8MB
                        maxConcurrent: 6,
                        connectionTimeout: 15000,
                        retryCount: 2
                    );
                    
                default:
                    return CreateHttpDownloader(); // 默认配置
            }
        }

        private IDownloader CreateHttpDownloader(
            int chunkSize = 2 * 1024 * 1024,
            int maxConcurrent = 4,
            int connectionTimeout = 20000,
            int retryCount = 3)
        {
            // 这里暂时返回基础实现，等待HttpDownloader完善
            return new UniHttpDownloader();
        }
    }
}