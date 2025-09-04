
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
        public void Update(int deltaTime);

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
        /// <returns></returns>
        public int AddTask(string fileName);

        /// <summary>
        /// 添加下载任务，可加载回调
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="finish"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public int AddTask(string fileName, Action<bool> finish, Action<int> process);

        /// <summary>
        /// 停止任务
        /// </summary>
        /// <param name="taskId"></param>
        public void StopTask(int taskId);

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="taskId"></param>
        public void PauseTask(int taskId);

        /// <summary>
        /// 恢复任务
        /// </summary>
        /// <param name="taskId"></param>
        public void ResumeTask(int taskId);

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="taskId"></param>
        public void RemoveTask(int taskId);

        // 释放
        public void Dispose();
    }
}