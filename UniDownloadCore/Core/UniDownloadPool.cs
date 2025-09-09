using System;
using System.Collections.Concurrent;
using System.Threading;

namespace UniDownload
{
    /// <summary>
    /// 通用对象池，支持实现IPoolable接口的类型
    /// </summary>
    internal class UniDownloadPool
    {
        private object _lock = new object();
        private ConcurrentQueue<IPoolable> _pool;

        // 池配置
        private int _maxPoolSize = 20;  // 最大池大小
        private int _currentCount = 0;   // 当前池中对象数量
        
        /// <summary>
        /// 设置池的最大大小
        /// </summary>
        public UniDownloadPool(int maxSize = 8)
        {
            _maxPoolSize = Math.Max(1, maxSize);
            _pool = new ConcurrentQueue<IPoolable>();
        }
        
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Rent<T>() where  T : IPoolable, new()
        {
            T result = default(T);
            
            foreach (IPoolable item in _pool)
            {
                if (item is T findResult)
                {
                    result = findResult;
                    Interlocked.Decrement(ref _currentCount);
                    break;
                }
            }

            if (result == null)
            {
                result = new T();
            }

            result.OnRentFromPool();
            return result;
        }
        
        /// <summary>
        /// 将对象返回到池中
        /// </summary>
        public void Return(IPoolable item)
        {
            if (item == null) return;
            
            // 重置对象状态
            item.OnReturnToPool();
            
            // 检查池大小限制
            if (_currentCount < _maxPoolSize)
            {
                _pool.Enqueue(item);
                Interlocked.Increment(ref _currentCount);
                return;
            }

            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// 获取池的统计信息
        /// </summary>
        public int GetUsedNum()
        {
            return _currentCount;
        }

        /// <summary>
        /// 清空池
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                while (_pool.TryDequeue(out var value))
                {
                    if (value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _currentCount = 0;
            }
        }
    }
}