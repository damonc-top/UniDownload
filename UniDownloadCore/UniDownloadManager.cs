using System;

namespace UniDownload.UniDownloadCore
{
    public class UniDownloadManager
    {
        private UniDownloadRequestScheduler _requestScheduler;
        private UniDownloadTaskScheduler _taskScheduler;
        
        public UniDownloadManager()
        {
            Initialize();
            RegistrationService();
        }

        // 开始运行
        public void StartRun()
        {
            _requestScheduler.Start();
            _taskScheduler.Start();
        }

        // 主线程调用
        public void Update()
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
            var result = _requestScheduler.RemoveRequest(uuid);
            if (result.IsSuccess)
            {
                _taskScheduler.StopTask(result.Value);    
            }
        }

        // 初始化分层调度器
        private void Initialize()
        {
            _taskScheduler = new UniDownloadTaskScheduler();
            _requestScheduler = new UniDownloadRequestScheduler(_taskScheduler);
        }
        
        // 注册全局服务对象
        private void RegistrationService()
        {
            UniServiceContainer.Register(new UniMainThread());
            UniServiceContainer.Register(new UniDownloadSetting());
            UniServiceContainer.Register(new UniDownloadNetwork());
            UniServiceContainer.Register(new UniSegmentWorker());
            UniServiceContainer.Register(new UniTaskScheduler());
        }
    }
}
