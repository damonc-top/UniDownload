using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Collections.Concurrent;

namespace UniDownload.UniDownloadCore
{
    internal class UniMD5
    {
        private static readonly ThreadLocal<MD5> _threadLocalMD5 = new ThreadLocal<MD5>(CreateMD5);
        private static readonly ConcurrentQueue<byte[]> _bufferPool = new ConcurrentQueue<byte[]>();
        private static readonly ConcurrentQueue<char[]> _hexCharPool = new ConcurrentQueue<char[]>();
        private const int BUFFER_SIZE = 8192; // 8KB缓冲区
        private const int HEX_SIZE = 32; //十六进制字符串长度
        
        
        public Result<bool> VerifyMD5Hash(string srcMd5Hash, string dstFilePath)
        {
            if (string.IsNullOrEmpty(srcMd5Hash) || string.IsNullOrEmpty(dstFilePath))
                return Result<bool>.Success(true);
                
            try
            {
                string actualHash = ComputeFileHash(dstFilePath);
                return Result<bool>.Success(string.Equals(srcMd5Hash, actualHash, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"MD5文件{dstFilePath} 校验失败: {ex.Message}");
            }
        }

        private string ComputeFileHash(string dstFilePath)
        {
            // 获取当前线程的MD5实例
            var md5 = _threadLocalMD5.Value;
            
            // 从对象池获取缓冲区，或创建新的
            if (!_bufferPool.TryDequeue(out byte[] buffer))
            {
                buffer = new byte[BUFFER_SIZE];
            }

            if (!_hexCharPool.TryDequeue(out char[] hexChar))
            {
                hexChar = new char[HEX_SIZE];
            }

            try
            {
                using var fileStream = new FileStream(dstFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                md5.Initialize(); // 重置MD5状态
                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                }

                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                for (int i = 0; i < md5.Hash.Length; i++)
                {
                    byte b = md5.Hash[i];
                    hexChar[i * 2] = GetHexChar(b >> 4);
                    hexChar[i * 2 + 1] = GetHexChar(b & 0xF);
                }

                return new string(hexChar);
            }
            finally
            {
                // 将缓冲区返回对象池
                int max = UniUtils.GetMaxParallel() * 2;
                if (_bufferPool.Count < max)
                {
                    _bufferPool.Enqueue(buffer);
                }

                if (_hexCharPool.Count < max)
                {
                    _hexCharPool.Enqueue(hexChar);
                }
            }
        }

        // 静态方法：直接计算文件MD5
        public static Result<string> ComputeFileMD5(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Result<string>.Fail($"计算md5的文件找不到 {filePath}");
            }

            return Result<string>.Success(new UniMD5().ComputeFileHash(filePath));
        }

        // 清理资源
        public static void Dispose()
        {
            if (_threadLocalMD5.IsValueCreated)
            {
                _threadLocalMD5.Value?.Dispose();
            }
            _threadLocalMD5.Dispose();
        }
        
        // 转16进制
        private static char GetHexChar(int value)
        {
            //48='0',97='a'
            return (char)(value < 10 ? 48 + value : 97 + value - 10);
        }
        
        private static MD5 CreateMD5()
        {
            return MD5.Create();
        }
    }
}