namespace UniDownload
{
    /*
        下载配置 
    */
    internal class UniDownloadSetting
    {        
        /// <summary>
        /// 源站URL
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// 保存根路径
        /// </summary>
        public string BaseSavePath { get; set; }

        /// <summary>
        /// 临时保存根路径
        /// </summary>
        public string BaseSaveTempPath { get; set; }

        /// <summary>
        /// 最大调度并发数
        /// </summary>
        public int MaxScheduleParallel { get; set; } = 4;

        /// <summary>
        /// 最大头文件请求并发数
        /// </summary>
        public int MaxHeadParallel { get; set; } = 1;

        /// <summary>
        /// 最大下载并发数
        /// </summary>
        public int MaxDownloadParallel { get; set; } = 2;

        /// <summary>
        /// 最大IO并发数
        /// </summary>
        public int MaxIOParallel { get; set; } = 1;

        /// <summary>
        /// 分片大小（字节）
        /// </summary>
        public int ChunkSize { get; set; } = 1 * 1024 * 1024; // 默认1MB

        /// <summary>
        /// 连接超时时间（毫秒）
        /// </summary>
        public int ConnectionTimeout { get; set; } = 60000; // 默认60秒

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// 速度限制（字节/秒）0表示不限制
        /// </summary>
        public int SpeedLimitBytesPerSecond { get; set; } = 0;

        /// <summary>
        /// 是否启用断点续传
        /// </summary>
        public bool EnableSegments { get; set; } = true;
    }
}