namespace UniDownload.UniDownloadCore.Common
{
    public interface IPoolable
    {
        public void Rent();
        public void Return();
        public void Release();
    }
}