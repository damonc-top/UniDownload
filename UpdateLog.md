

## 下载管理器

UniDownloadManager : IDisposable
    根据下载配置，下载任务调度器、下载任务处理者
    

## 下载数据事件参数

UniDownloadDataEventArgs : EventArgs

## 下载调度器

UniDownloadTaskScheduler : IDownloadTaskScheduler

//任务处理者
UniDownloadTaskWorker : IDownloadTaskWorker

## 下载任务

UniDownloadTask : IDisposable

UniDownloadState

UniDownloadSpeedTracker : IDownloadSpeedTracker

## 下载线程

UniDownloadThread : IDownloadThread

UniDownloadThreadManager : IDownloadThreadManager


## 工厂类

UniDownloadThreadManagerFactory : IDownloadThreadManagerFactory
UniDownloadThreadFactory : IDownloadThreadFactory
UniDownloadServiceFactory : IDownloadServiceFactory

## 协议类

Http
    UniHttpDownloadContext : IDownloadContext
    UniHttpDownloadService : IDownloadService
    UniHttpFileDownloader : IFileDownloader
Socket
    UniSocketDownloadContext : IDownloadContext
    UniSocketDownloadService : IDownloadService
    UniSocketFileDownloader : IFileDownloader

## 异常类

UniRequesetConnectionException : Exception
UniRequesetResponseException : Exception
UniDownloadContextInvaildException : Exception
...

## 日志类

UniDownloadLogger

UniDownloadLogging : IDownloadLogging

