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
            _taskScheduler.Update();
            _requestScheduler.Update();
            UniServiceContainer.Get<UniMainThread>().Update();
        }

        // 停止运行
        public void StopRun()
        {
            _requestScheduler.Stop();
            _taskScheduler.Stop();
        }

        // 添加请求
        public int AddRequest(string fileName, bool isHighest, Action finish, Action<int> progress)
        {
            return _requestScheduler.AddRequest(fileName, isHighest, finish, progress);
        }

        // 移除请求
        public void RemoveRequest(int uuid)
        {
            _requestScheduler.RemoveRequest(uuid);
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
