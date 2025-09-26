
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadSegmentManager
    {
        private CancellationTokenSource _cancellation;
        private IDownloadContext _context;
        private UniDownloadSegment[] _fileSegmentThread;
        private int _completeNum;
        private int _maxRetry;
        private int[] _retryNums;
        private SemaphoreSlim _downloadSemaphore;
        
        public UniDownloadSegmentManager(IDownloadContext context)
        {
            _completeNum = 0;
            _context = context;
            _downloadSemaphore = new SemaphoreSlim(1, 1);
            CreateSegments();
        }

        // 开始任务下载
        public void Start()
        {
            UniDownloadEventBus.SegmentCompleted += OnSegmentComplete;
            _cancellation = new CancellationTokenSource();
            StartDownloadSegment();
        }

        // 停止任务下载
        public async Task StopAsync()
        {
            // 互斥锁，最多一个许可运行
            await _downloadSemaphore.WaitAsync();
            try
            {
                // 取消所有
                _cancellation?.Cancel();
            
                List<Task> stopSegments = new List<Task>();
                for (int i = 0; i < _fileSegmentThread.Length; i++)
                {
                    stopSegments.Add(StopSegmentAsync(_fileSegmentThread[i], i));
                }
                
                // 等待所有segment停止（最多10秒）
                using (_ = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    try
                    {
                        await Task.WhenAll(stopSegments).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        UniLogger.Error("某些segment未能在超时时间内停止");
                    }
                }
                
                UniDownloadEventBus.SegmentCompleted -= OnSegmentComplete;
                
                _cancellation?.Dispose();
                _cancellation = null;
            }
            finally
            {
                _downloadSemaphore.Release();
            }

        }
        // 终止所有segment下载
        private async Task StopSegmentAsync(UniDownloadSegment segment, int index)
        {
            try
            {
                await Task.Run(segment.Dispose, _cancellation?.Token ?? CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                UniLogger.Error($"停止segment {index} 失败: {ex.Message}");
            }
        }
        
        // 创建文件分段对象
        private void CreateSegments()
        {
            _maxRetry = _context.MaxParallel;
            _retryNums = new int[_context.MaxParallel];
            int count = _context.SegmentFiles.Length;
            _fileSegmentThread = new UniDownloadSegment[count];
            for (int i = 0; i < count; i++)
            {
                _fileSegmentThread[i] = new UniDownloadSegment(_context.FileName, _context.SegmentFiles[i]);
            }
        }

        // 开始下载分段文件
        private void StartDownloadSegment()
        {
            UniSegmentWorker worker = UniServiceContainer.Get<UniSegmentWorker>();
            Result<Stream[]> segmentStreams = worker.GetSegmentStreams(_segmentTempPaths, _context);
            if (!segmentStreams.IsSuccess)
            {
                UniLogger.Error($"文件分段错误 {_context.FileName}");
                return;
            }

            Stream[] writeStreams = segmentStreams.Value;
            for (int i = 0; i < writeStreams.Length; i++)
            {
                // TODO 1
                _fileSegmentThread[i].Start(writeStreams[i], 0, _context.SegmentRanges[i, 1], _cancellation.Token);
            }
        }

        // 分段文件下载回调
        private void OnSegmentComplete(object sender, UniDownloadEventArgs args)
        {
            if (args.RequestId != _context.RequestId) return;
            if (args.ErrorMessage != null)
            {
                // 分段文件下载有错误信息处理
                OnSegmentFailed(args.RequestId, args.ErrorMessage);
                return;
            }

            // 任意一个分段文件重试次数达到上限，判定该文件下载失败
            Interlocked.Increment(ref _completeNum);
            if (_completeNum >= _segmentTempPaths.Length)
            {
                UniSegmentWorker worker = UniServiceContainer.Get<UniSegmentWorker>();
                var mergeResult = worker.MergeSegmentFiles(_context, _segmentTempPaths);

                if (!mergeResult.IsSuccess)
                {
                    // TODO 合并文件失败了 要重新下载
                    return;
                }
                
                Task.Factory.StartNew(BroadcastAsync, null, _cancellation.Token, TaskCreationOptions.None, UniServiceContainer.Get<UniTaskScheduler>());
            }
        }

        // 下载分段文件失败
        private void OnSegmentFailed(int segmentIndex, string errorMessage)
        {
            UniLogger.Error(errorMessage);
            // 分段重试次数有效
            if (_retryNums[segmentIndex] < _maxRetry)
            {
                _retryNums[segmentIndex]++;
                Task.Factory.StartNew(RetryDownloadSegmentAsync, segmentIndex, _cancellation.Token, TaskCreationOptions.None, UniServiceContainer.Get<UniTaskScheduler>());
                return;
            }
            
            // 某一个分段重试完成表示资源始终下载失败，这时要放弃下载整个资源，执行停止逻辑
            Task.Factory.StartNew(BroadcastAsync, errorMessage, _cancellation.Token, TaskCreationOptions.None, UniServiceContainer.Get<UniTaskScheduler>());
        }

        private async void RetryDownloadSegmentAsync(object state)
        {
            try
            {
                int segmentIndex = (int) state;
                int delayMs = (int) Math.Pow(2, _retryNums[segmentIndex] - 1) * 1000;
                await Task.Delay(delayMs);
                if (_cancellation.Token.IsCancellationRequested)
                {
                    // TODO 退出下载
                    return;
                }

                _fileSegmentThread[segmentIndex].Dispose();
                UniSegmentWorker worker = UniServiceContainer.Get<UniSegmentWorker>();
                var result = worker.GetSegmentStream(_segmentTempPaths[segmentIndex], 0);
                if (!result.IsSuccess)
                {
                    UniLogger.Error($"重试获取 Segment {segmentIndex} 流失败: {result.Message}");
                    OnSegmentFailed(segmentIndex, result.Message);
                    return;
                }

                long start = _context.SegmentRanges[segmentIndex, 0];
                long end = _context.SegmentRanges[segmentIndex, 1];
                _fileSegmentThread[segmentIndex] = new UniDownloadSegment(segmentIndex, _context.FileName);
                _fileSegmentThread[segmentIndex].Start(result.Value, start, end, _cancellation.Token);
            }
            catch (OperationCanceledException e)
            {
                // 取消操作，正常退出
                UniLogger.Error($"分段文件下载重试操作被取消 {e.Message}");
            }
            catch (Exception e)
            {
                UniLogger.Error(e.Message);
                _ = Task.Factory.StartNew(BroadcastAsync, e.Message, _cancellation.Token, TaskCreationOptions.None, UniServiceContainer.Get<UniTaskScheduler>()).ConfigureAwait(false);
            }
        }

        // 从其他线程池切到自定义的线程调度器，避免过度占用其他线程池
        private async void BroadcastAsync(object state)
        {
            string fileName = _context.FileName;;
            try
            {
                await StopAsync();
                UniDownloadEventBus.RaiseDownloadTaskCompleted(_context.RequestId, _context.FileName, state as string);
            }
            catch (Exception e)
            {
                UniLogger.Error($"下载文件{fileName} 异常 {e.Message}");
            }
        }
    }
}