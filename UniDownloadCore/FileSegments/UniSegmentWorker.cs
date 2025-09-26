using System;
using System.Collections.Generic;
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
            // 基准分割size
            long standardSize = UniServiceContainer.Get<UniDownloadSetting>().SegmentSize;
            // 基准分割数
            int segmentNum = (int)Math.Ceiling((double)fileLength / standardSize);
            // 比较基准分割数和给定并发数，取最小数值并发
            int newParallel = Math.Min(segmentNum, maxParallel);
            // 重新计算分割size
            long segmentSize = fileLength / newParallel;
            // 剩余尾部一点size，合并给最后一个分割文件
            long remainSize = fileLength % newParallel;
            long[,] segmentPosition = new long[newParallel, 2];
            long startPos = 0;
            for (int i = 0; i < newParallel; i++)
            {
                long endPos = startPos + segmentSize - 1;
                if (i == newParallel - 1)
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
        public Result<string[]> GetSegmentPaths(IDownloadContext context)
        {
            List<string> segmentPaths = new List<string>();
            foreach (UniSegmentFile segmentFile in context.SegmentFiles)
            {
                if (!segmentFile.IsDone)
                {
                    segmentPaths.Add(Path.Combine(context.FileTempRootPath, segmentFile.SegName));
                }
            }
            
            return Result<string[]>.Success(segmentPaths.ToArray());
        }
        
        // 获取分段文件写入流，设置断点续传
        public Result<Stream[]> GetSegmentStreams(string[] segmentPaths, IDownloadContext context)
        {
            // TODO da
            long[] downloaded = new long[1];//context.SegmentDownloaded;
            if (segmentPaths.Length != downloaded.GetLength(0))
            {
                return Result<Stream[]>.Fail($"获取分段文件写入流失败：路径数组与已下载数组的长度不一致");
            }
            Stream[] writeSegmentStreams = new Stream[segmentPaths.Length];
            for (int i = 0; i < segmentPaths.Length; i++)
            {
                var result = GetSegmentStream(segmentPaths[i], downloaded[i]);
                writeSegmentStreams[i] = result.Value;
            }

            return Result<Stream[]>.Success(writeSegmentStreams);
        }

        public Result<Stream> GetSegmentStream(string segmentPath, long downloaded)
        {
            Stream stream = new FileStream(segmentPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            stream.Seek(downloaded, SeekOrigin.Begin);
            return Result<Stream>.Success(stream);
            
        }

        /// <summary>
        /// 分段文件合并
        /// </summary>
        /// <param name="context">下载上下文</param>
        /// <param name="segmentPaths">分段文件路径</param>
        /// <returns>是否成功</returns>
        public Result<bool> MergeSegmentFiles(IDownloadContext context, string[] segmentPaths)
        {
            int length = segmentPaths.Length;

            if (length == 1)
            {
                var md5Result = new UniMD5().VerifyMD5Hash(context.MD5Hash, segmentPaths[0]);
                if (!md5Result.IsSuccess)
                {
                    return Result<bool>.Fail(md5Result.Message);
                }
                MoveFile(segmentPaths[0], context.FilePath);
                CleanupFiles(segmentPaths);
                return Result<bool>.Success(true);
            }

            try
            {
                using FileStream final = new FileStream(context.FileTempPath, FileMode.Create, FileAccess.Write);
                byte[] buff = new byte[1024];
                for (int i = 0; i < length; i++)
                {
                    int readBytes;
                    using FileStream segment = new FileStream(segmentPaths[i], FileMode.Open, FileAccess.Read);
                    while ((readBytes = segment.Read(buff, 0, buff.Length)) > 0)
                    {
                        final.Write(buff, 0, readBytes);
                    }
                }

                final.Flush();
                final.Close();
                var md5Result = new UniMD5().VerifyMD5Hash(context.MD5Hash, context.FileTempPath);
                if (!md5Result.IsSuccess)
                {
                    return Result<bool>.Fail(md5Result.Message);
                }

                MoveFile(context.FileTempPath, context.FilePath);
                CleanupFiles(segmentPaths);
                return Result<bool>.Success(true);
            }
            catch (Exception e)
            {
                return Result<bool>.Fail($"{context.FileTempPath} 合并文件失败 {e.Message}");
            }
        }

        public void MoveFile(string src, string dst)
        {
            File.Move(src, dst);
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