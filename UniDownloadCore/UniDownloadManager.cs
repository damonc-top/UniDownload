using System;

namespace UniDownload
{
    /*
     * 资源下载管理器，提供下载、暂停、取消等接口
     * 只支持Unity 2020.x及以上的新版本
    */
    public class UniDownloadManager : IDisposable
    {
#if UNITY_2020_1_OR_NEWER
        private UniMainThread _mainThread;
        private IDownloadScheduler _downloadScheduler;
        
        /// <summary>
        /// 启动manager初始化基础路径
        /// </summary>
        /// <param name="baseURL">源站基础地址</param>
        /// <param name="baseSavePath">正式保存根路径</param>
        /// <param name="saveTempRootPath">下载临时根路径</param>
        public UniDownloadManager(string baseURL, string baseSavePath, string saveTempRootPath)
        {
            _downloadScheduler = new UniDownloadScheduler();
            _mainThread = new UniMainThread();
            UniServiceContainer.Register<UniMainThread>(_mainThread);
            UniServiceContainer.Register<UniDownloadSetting>(new UniDownloadSetting());
            UniServiceContainer.Register<UniDownloadPool>(new UniDownloadPool(20));
        }

        public void MainUpdate(float delta)
        {
            _downloadScheduler.Update(delta);
            _mainThread.Update();
        }

        public int AddRequest(string fileName)
        {
            return _downloadScheduler.AddRequest(fileName);
        }

        public int AddRequest(string fileName, Action<bool> finish, Action<int> process)
        {
            return _downloadScheduler.AddRequest(fileName, finish, process);
        }

        public void StopRequest(int requestID, object owner = null)
        {
            _downloadScheduler.StopRequest(requestID, owner);
        }

        public void PauseRequest(int uuid)
        {
            _downloadScheduler.PauseRequest(uuid);
        }

        public void Dispose()
        {
            _downloadScheduler?.Dispose();
            UniServiceContainer.Dispose();
        }
#endif
    }
}