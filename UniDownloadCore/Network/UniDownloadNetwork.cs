
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace UniDownload.UniDownloadCore
{
    internal class UniDownloadNetwork
    {
        private HttpClient _client;

        public UniDownloadNetwork()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                UseCookies = false,
            };
            _client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMilliseconds(UniUtils.GetTimeOut()),
                BaseAddress = new Uri(UniUtils.GetBaseURL()),
            };
        }

        // 获取远程文件的大小
        public async Task<Result<long>> GetRemoteFileLength(IDownloadContext context, CancellationToken token)
        {
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Head, context.FileName))
            {
                try
                {
                    var responseMessage = await _client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);
                    responseMessage.EnsureSuccessStatusCode();
                    if (responseMessage.Content.Headers.ContentLength.HasValue)
                    {
                        var length = responseMessage.Content.Headers.ContentLength.Value;
                        return Result<long>.Success(length);
                    }

                    return Result<long>.Fail($"获取文件size失败 文件名 = {context.FileName} 状态码 = {responseMessage.StatusCode}");
                }
                catch (OperationCanceledException e)
                {
                    // 取消操作转换为失败Result
                    return Result<long>.Fail($"获取文件长度被取消: {context.FileName}");
                }
                catch (HttpRequestException e)
                {
                    // HTTP异常转换为失败Result
                    return Result<long>.Fail($"获取文件长度网络失败: {context.FileName}, {e.Message}");
                }
                catch (Exception e)
                {
                    // 其他异常转换为失败Result
                    return Result<long>.Fail($"获取文件长度其他异常: {context.FileName}, {e.Message}");
                }
            }
        }

        // 获取远程文件段读取流
        public async Task<Result<Stream>> GetResponseStream(string fileName, long startRange, long endRange,
            CancellationToken token)
        {
            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, fileName))
            {
                try
                {
                    message.Headers.Range = new RangeHeaderValue(startRange, endRange);
                    var response = await _client.SendAsync(message, HttpCompletionOption.ResponseContentRead, token);
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStreamAsync();
                    return Result<Stream>.Success(result);
                }                
                catch (OperationCanceledException e)
                {
                    throw;
                }
                catch (HttpRequestException e)
                {
                    return Result<Stream>.Fail($"创建文件读取流网络请求失败，文件名: {fileName}, 错误: {e.Message}");
                }
                catch (Exception e)
                {
                    return Result<Stream>.Fail($"创建文件读取流异常，文件名: {fileName}, {e.Message}");
                }
            }
        }
    }
}