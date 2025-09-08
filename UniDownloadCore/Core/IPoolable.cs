namespace UniDownload
{
    
    /// <summary>
    /// 池化对象必须实现的接口
    /// </summary>
    internal interface IPoolable
    {
        /// <summary>
        /// 从池中租用时调用
        /// </summary>
        void OnRentFromPool();
        
        /// <summary>
        /// 返回到池中时调用（用于重置状态）
        /// </summary>
        void OnReturnToPool();
    }
}