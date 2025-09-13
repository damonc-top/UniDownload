namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadSetting
    {
        // 最大并发数
        public int MaxParallel { get; set; } = 4;

        // 文件段
        public long SegmentSize { get; set; } = 1 * 512 * 1024; // 512kb分割

        public int SegmentParallel { get; set; } = 2;
        
        // 下载超时时间
        public int TimeOut { get; set; } = 60 * 1000;
        
        // 版本号或者时间戳能作为版本标记的值
        public string Version { get; set; }

        // 正式保存路径
        public string BaseSavePath { get; set; }
        
        // 临时保存路径
        public string TeamSavePath { get; set; }
        
        // 源站地址，需要以斜杠结尾
        public string BaseURL { get; set; }
        
        // 请求的存活时间
        public int RequestLifeTime { get; set; }
    }
}