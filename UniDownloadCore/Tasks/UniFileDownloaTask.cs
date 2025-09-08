using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload
{
    /*
        文件下载任务
    */
    internal class UniFileDownloadTask : IDisposable
    {
        // 任务UUID
        private int _uuid;

        // 任务状态
        private UniDownloadState _state;

        // 文件信息摘要
        private readonly UniDownloadFileInfo _fileInfo;

        // private UniDownloadService _service;

        private CancellationTokenSource _queryCancellation;
        
        // 任务时间戳
        private int _timestamp;
        

        private UniFileDownloadTask(int uuid, UniDownloadFileInfo fileInfo)
        {
            _uuid = uuid;
            _fileInfo = fileInfo;
            _state = UniDownloadState.Prepare;
        }

        /// <summary>
        /// 开始执行任务下载
        /// </summary>
        public void Start()
        {
            _state = UniDownloadState.Querying;
            _queryCancellation = new CancellationTokenSource();
            Task.Run(CreateDownloadContext, _queryCancellation.Token);
        }

        private async void CreateDownloadContext()
        {
            try
            {
                HttpClient client = new HttpClient();
                Task<HttpResponseMessage> task = client.SendAsync(new HttpRequestMessage());
                await task;
            }
            catch (Exception e)
            {
                //TODO head链接异常处理
                UniLogger.Error(e.Message);
            }
        }
        
        /// <summary>
        /// 暂停任务下载
        /// </summary>
        /// <param name="taskId"></param>
        public void Pause(int taskId)
        {
            _state = UniDownloadState.Paused;
        }

        /// <summary>
        /// 恢复任务下载
        /// </summary>
        /// <param name="taskId"></param>
        public void Resume(int taskId)
        {
            _state = UniDownloadState.Downloading;
        }

        /// <summary>
        /// 添加下载完成与进度回调，同一个文件下载任务可以接受不同的请求者回调函数
        /// </summary>
        /// <param name="finish">下载完成回调函数</param>
        /// <param name="process">下载进度回调函数</param>
        /// <returns>返回当次下载操作ID，便于请求者取消下载任务回调</returns>
        public int AddAction(Action<bool> finish, Action<int> process)
        {
            int actionId = 0;

            return actionId;
        }

        /// <summary>
        /// 移除下载任务中的完成与进度回调函数
        /// </summary>
        /// <param name="actionId"></param>
        public void RemoveAction(int actionId)
        {

        }
        

        /// <summary>
        /// 获取任务ID
        /// </summary>
        /// <returns>返回任务ID</returns>
        public int GetTaskID()
        {
            return _uuid;
        }

        /// <summary>
        /// 创建文件下载任务
        /// </summary>
        /// <param name="filename">文件相对路径，eg:Bundles/Android/xxx</param>
        /// <returns>UniFileDownloadTask</returns>
        public static UniFileDownloadTask Create(string filename)
        {
            //TODO 计算文件MD5值
            string md5 = "";
            string newUrl = "";//$"{UniSetting.RootServerUrl}/{filename}";
            string savePath = "";//$"{UniSetting.RootSavePath}/{filename}";
            UniDownloadFileInfo fileInfo = new UniDownloadFileInfo(filename, newUrl, savePath, md5);
            return new UniFileDownloadTask(UniID.ID, fileInfo);
        }
        
        /// <summary>
        /// 释放文件下载任务
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}