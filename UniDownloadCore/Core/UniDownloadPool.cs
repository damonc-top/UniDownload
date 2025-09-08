using System;
using System.Collections.Concurrent;
using System.Threading;

namespace UniDownload
{
    /// <summary>
    /// 通用对象池，支持任何实现IPoolable接口的类型
    /// </summary>
    internal class UniDownloadPool<T> where T : IPoolable
    {
        private object _lock = new object();
        private ConcurrentQueue<T> _pool;

        // 池配置
        private int _maxPoolSize = 20;  // 最大池大小
        private int _currentCount = 0;   // 当前池中对象数量
        
        /// <summary>
        /// 设置池的最大大小
        /// </summary>
        public UniDownloadPool(int maxSize)
        {
            _maxPoolSize = Math.Max(1, maxSize);
            _pool = new ConcurrentQueue<T>();
        }
        
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public T Rent()
        {
            if (_pool.TryDequeue(out T item))
            {
                Interlocked.Decrement(ref _currentCount);
                item.OnRentFromPool();
                return item;
            }
            
            // 池中没有对象，创建新的
            var newItem = Activator.CreateInstance<T>();
            newItem.OnRentFromPool();
            return newItem;
        }
        
        /// <summary>
        /// 将对象返回到池中
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;
            
            // 重置对象状态
            item.OnReturnToPool();
            
            // 检查池大小限制
            if (_currentCount < _maxPoolSize)
            {
                _pool.Enqueue(item);
                Interlocked.Increment(ref _currentCount);
            }
            // 超出限制的对象直接丢弃，让GC处理
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
                    Interlocked.Decrement(ref _currentCount);
                    if (value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}