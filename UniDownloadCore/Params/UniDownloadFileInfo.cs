
namespace UniDownload
{
    internal readonly struct UniDownloadFileInfo
    {
        // 下载地址
        public readonly string Url;

        // 文件名
        public readonly string FileName;

        // 保存路径
        public readonly string SavePath;

        // 文件md5
        public readonly string MD5;
        
        public UniDownloadFileInfo(string fileName, string url, string savePath, string md5)
        {
            FileName = fileName;
            Url = url;
            SavePath = savePath;
            MD5 = md5;
        }
    }
}