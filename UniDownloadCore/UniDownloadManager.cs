using System;

namespace UniDownload.UniDownloadCore
{
    public class UniDownloadManager
    {
        private UniDownloadRequestScheduler _requestScheduler;
        private UniDownloadTaskScheduler _taskScheduler;
        
        public UniDownloadManager()
        {
            RegistrationService();
        }

        // 开始运行
        public void StartRun()
        {
            
        }

        // 主线程调用
        public void Update(float delta)
        {
            _requestScheduler.Update();
            _taskScheduler.Update();
            UniServiceContainer.Get<UniMainThread>().Update();
        }

        // 停止运行
        public void StopRun()
        {
            _requestScheduler.Stop();
            _taskScheduler.Stop();
        }

        public int AddRequest(string fileName, bool isHighest, Action finish, Action<int> progress)
        {
            return _requestScheduler.AddRequest(fileName, isHighest, finish, progress);
        }

        public void RemoveRequest(int uuid)
        {
            _requestScheduler.RemoveRequest(uuid);
        }

        private void RegistrationService()
        {
            UniServiceContainer.Register(new UniMainThread());
            UniServiceContainer.Register(new UniDownloadSetting());
        }
    }
}