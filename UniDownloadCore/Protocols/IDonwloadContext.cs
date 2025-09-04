namespace UniDownload
{
    /*
        下载上下文
    */
    internal interface IDownloadContext
    {
        public string ServerUrl { get; }
        public string SavePath { get; }
        public int[][] RangePosition { get; }
    }
}