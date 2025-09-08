namespace UniDownload
{
    /// <summary>
    /// 下载器工厂接口
    /// </summary>
    internal interface IDownloaderFactory
    {
        /// <summary>
        /// 根据网络状况智能选择最佳下载器
        /// </summary>
        /// <param name="networkSpeed">网络速度</param>
        /// <param name="networkType">网络类型</param>
        /// <returns>下载器实例</returns>
        public IDownloader Create(NetworkSpeed networkSpeed, NetworkType networkType);
    }
}