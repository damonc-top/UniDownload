
进行模块化封装，对外提供下载、暂停、通知进度等

## 下载管理器

UniDownloadManager : IDisposable
    是对外API接口
    包含接口有：
        void 初始化Manager(serverBase, maxParallel) //maxParallel会根据设备性能分级二次优化
        uuid 添加下载文件(url, finish<bool>)               //url暂定是相对路径
        bool 暂停/取消任务(uuid)
        bool 取消全部任务()
        void Dispose()
        EventHandler DownloadComplete
        EventHandler DownloadProcess

## 下载数据事件参数

UniDownloadDataEventArgs : EventArgs
    下载数据事件通知，暂定
    UniFileDownloader

## 下载调度器

UniDownloaderScheduler : IDownloaderScheduler
    文件下载调度器，暴露给UniDownloadManager。控制文件下载并发、添加移除、开始暂停恢复、事件通知
    包含接口有：
        void 初始化(maxParallel, protocol, [worker])
        uuid 添加下载文件(url, finish<bool>)
        bool 暂停/取消任务(uuid)
        bool 取消全部任务()
        事件通知


UniDownloadWoker : IDownloadWoker
    任务处理器，文件下载实际操作
    包含接口有：
        处理分段


## 下载文件

UniFileDownloaTask : IDisposable
    下载文件的包装类，管理文件下载地址、保存地址、文件md5、文件分段、控制Task下载与暂停、事件
    包含接口有：
        文件分段task管理器
        下载状态
        static Create()
        int GetUUID()
        bool 开始任务()
        bool 暂停/取消任务()
        bool 取消全部任务()



UniDownloadState
    下载状态：
        Prepare
        Downloading
        PostDownloading
        Paused
        Cancelled
        Completed
        Failed
        

UniDownloadSpeedTracker : IDownloadSpeedTracker
    下载速度报告

## 下载任务

UniDownloader
    绑定task，开始、暂定、取消


UniDownloaderManager : IDownloaderManager
    管理UniDownloader，暴露给UniFileDownloader

## 工厂类

UniDownloaderManagerFactory : IDownloaderManagerFactory
    创建IDownloadTaskManager

UniDownloaderFactory : IDownloaderFactory
    创建UniDownloader

UniDownloadServiceFactory : IDownloadServiceFactory
    创建IDownloadService

## 协议类
    IDownloadContext
        md5、保存路径、分段数据
    IDownloadService
        主要是网络交互接口，比如获取remote length、标头分段、response Stream等等
    IDownloader
        主要是处理RemoteStream保存到本地


Http
    UniHttpDownloadContext : IDownloadContext
    UniHttpDownloadService : IDownloadService
    UniHttpDownloader : IDownloader
Socket
    UniSocketDownloadContext : IDownloadContext
    UniSocketDownloadService : IDownloadService
    UniSocketDownloader : IDownloader

## 异常类

UniRequesetConnectionException : Exception
UniRequesetResponseException : Exception
UniDownloadContextInvaildException : Exception
...

## 日志类

UniDownloadLogger

UniDownloadLogging : IDownloadLogging

public void Start(){
    GetLinkFileSizeAsync("123.com")
}

public static async Task<Result<long>> GetLinkFileSizeAsync(string link)
{}