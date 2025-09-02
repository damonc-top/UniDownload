
namespace UniDownload.Config
{
    /*
        下载配置
    */
    internal class DownloadConfig
    {
        public DevicePerformance Performance { get; set; }
        public NetworkType NetworkType { get; set; }
        public NetworkSpeed NetworkSpeed { get; set; }
        public ProtocolService ProtocolService { get; set; }
    }
}