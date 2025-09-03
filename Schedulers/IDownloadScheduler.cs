
namespace UniDownload.Schedulers
{
    /*
        下载调度器
    */
    internal interface IDownloadScheduler
    {
        

        public int AddTask(IDownloadContext context);

        public void Start();

        public void Stop();

        public void StopTask();

        public void PauseTask();

        public void ResumeTask();

        public void Dispose();
    }
}