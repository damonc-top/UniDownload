using System;
using System.IO;

namespace UniDownload.UniDownloadCore
{
    internal class UniSegmentWorker
    {
        // 对文件进行分段，可能文件比较小，就根据指定的标准分段尺寸进行分段，计算出来最小的分段并发数，不会超过文件最大的分段并发数
        // eg.文件5m，标准分段尺寸512k，那么只分SegmentParallel段，math.min(ceilInt(5m/512k), SegmentParallel)
        // eg.文件256k，标准分段尺寸512k，那么只分一段，math.min(ceilInt(256k/512k), SegmentParallel)
        public Result<long[,]> GetSegmentRange(long fileLength, int maxParallel)
        {
            if (fileLength < 1 || maxParallel < 1)
            {
                return Result<long[,]>.Fail($"文件分割错误， size={fileLength} maxParallel={maxParallel}");
            }
            
            long standardSize = UniServiceContainer.Get<UniDownloadSetting>().SegmentSize;
            int segmentNum = (int)Math.Ceiling((double)fileLength / standardSize);
            int parallel = Math.Min(segmentNum, maxParallel);
            long segmentSize = fileLength / parallel;
            long remainSize = fileLength % parallel;
            long[,] segmentPosition = new long[parallel, 2];
            long startPos = 0;
            for (int i = 0; i < parallel; i++)
            {
                long endPos = startPos + segmentSize - 1;
                if (i == parallel - 1)
                {
                    endPos += remainSize;
                }

                segmentPosition[i, 0] = startPos;
                segmentPosition[i, 1] = endPos;
                
                startPos = endPos + 1;
            }

            return Result<long[,]>.Success(segmentPosition);
        }

        // 获取文件分段路径，eg.临时保存路径/分段名.bin
        public Result<string[]> GetSegmentPaths(int parallel, string target)
        {
            if (parallel < 1 || string.IsNullOrEmpty(target))
            {
                return Result<string[]>.Fail($"获取文件分段路径失败");
            }
            string[] segmentPaths = new string[parallel];
            for (int i = 0; i < parallel; i++)
            {
                segmentPaths[i] = Path.Combine(target, UniUtils.GetSegmentName(i));
            }
            return Result<string[]>.Success(segmentPaths);
        }
        
        // 获取分段文件写入流，设置断点续传
        public Result<Stream[]> GetSegmentStream(string[] segmentPaths, IDownloadContext context)
        {
            long[] downloaded = context.SegmentDownloaded;
            if (segmentPaths.Length != downloaded.GetLength(0))
            {
                return Result<Stream[]>.Fail($"获取分段文件写入流失败：路径数组与已下载数组的长度不一致");
            }
            Stream[] writeSegmentStreams = new Stream[segmentPaths.Length];
            for (int i = 0; i < segmentPaths.Length; i++)
            {
                FileStream stream = new FileStream(segmentPaths[i], FileMode.OpenOrCreate, FileAccess.ReadWrite);
                stream.Seek(downloaded[i], SeekOrigin.Begin);
                writeSegmentStreams[i] = stream;
            }

            return Result<Stream[]>.Success(writeSegmentStreams);
        }

        // 分段文件合并
        public Result<bool> MergeSegmentFiles(string fileName, string[] segmentPaths)
        {
            int length = segmentPaths.Length;
            string targetFile = Path.Combine(UniUtils.GetBaseSavePath(), fileName);
            if (length == 1)
            {
                File.Move(segmentPaths[0], targetFile);
                CleanupFiles(segmentPaths);
                return Result<bool>.Success(true);
            }

            try
            {
                using FileStream final = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
                byte[] buff = new  byte[1024];
                for (int i = 0; i < length; i++)
                {
                    int readBytes;
                    using FileStream segment = new FileStream(segmentPaths[i], FileMode.Open, FileAccess.Read);
                    do
                    {
                        readBytes = segment.Read(buff, 0, buff.Length);
                        final.Write(buff, 0, readBytes);
                    } while (readBytes > 0);
                }
                final.Flush();
                final.Close();
                CleanupFiles(segmentPaths);
                return Result<bool>.Success(true);
            }
            catch (Exception e)
            {
                return Result<bool>.Fail($"{fileName} 合并文件失败 {e.Message}");
            }
        }

        // 清理下载的缓存文件
        private void CleanupFiles(string[] paths)
        {
            try
            {
                foreach (string filePath in paths)
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}