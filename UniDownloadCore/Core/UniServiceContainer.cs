using System;
using System.Collections.Concurrent;

namespace UniDownload
{
    /// <summary>
    /// 服务容器注入全局配置
    /// </summary>
    internal static class UniServiceContainer
    {
        private static readonly object _lock = new object();
        private static readonly ConcurrentDictionary<Type, object> _service = new ConcurrentDictionary<Type, object>();

        public static void Register<T>(T instance)
        {
            _service[typeof(T)] = instance;
        }

        public static T Get<T>()
        {
            if(_service.TryGetValue(typeof(T), out var instance))
            {
                return (T)instance;
            }

            return default;
        }
        /// <summary>
        /// 释放所有服务
        /// </summary>
        public static void Dispose()
        {
            lock (_lock)
            {
                foreach (var item in _service)
                {
                    if (item.Value is IDisposable disposable)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            UniLogger.Error($"释放service container {ex.Message}");
                        }
                    }
                }
                
                _service.Clear();
            }
        }
    }
}