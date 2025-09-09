
using System;

namespace UniDownload
{
    /*
        下载调度器
    */
    internal interface IDownloadScheduler
    {
        /// <summary>
        /// 更新调度器
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime);

        /// <summary>
        /// 开始调度
        /// </summary>
        public void Start();
        
        /// <summary>
        /// 停止调度
        /// </summary>
        public void Stop();

        /// <summary>
        /// 添加下载任务
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>任务ID</returns>
        public int AddRequest(string fileName);

        /// <summary>
        /// 添加下载任务，可加载回调
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="finish"></param>
        /// <param name="process"></param>
        /// <returns>任务ID</returns>
        public int AddRequest(string fileName, Action<bool> finish, Action<int> process);

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="uuid">请求ID</param>
        /// <param name="owner">模块</param>
        public void StopRequest(int uuid, object owner);

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="uuid"></param>
        public void PauseRequest(int uuid);

        /// <summary>
        /// 恢复任务
        /// </summary>
        /// <param name="uuid"></param>
        public void ResumeRequest(int uuid);

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="uuid"></param>
        public void RemoveRequest(int uuid);

        // 释放
        public void Dispose();
    }
}