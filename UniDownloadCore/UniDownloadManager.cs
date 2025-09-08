using System;

namespace UniDownload
{
    /*
     * 资源下载管理器，提供下载、暂停、取消等接口
    */
    public class UniDownloadManager : IDisposable
    {
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
            UniMainThread uniMainThread = UniUtils.CreateUniMainThread();
            UniServiceContainer.Register<UniMainThread>(uniMainThread);
            UniDownloadSetting setting = new UniDownloadSetting();
            UniServiceContainer.Register<UniDownloadSetting>(setting);
        }

        public int AddTask(string fileName)
        {
            return _downloadScheduler.AddTask(fileName);
        }

        public int AddTask(string fileName, Action<bool> finish, Action<int> process)
        {
            return _downloadScheduler.AddTask(fileName, finish, process);
        }

        public int AddTask(UniDownloadRequest request)
        {
            return _downloadScheduler.AddTask(request);
        }
        
        public void Dispose()
        {
            _downloadScheduler?.Dispose();
            UniServiceContainer.Dispose();
        }
    }
}