using UnityEngine;

namespace Aki_Test.Common
{
    /// <summary>
    /// 通用的泛型单例类，每一个需要用到单例模式的类都可以通过继承此类来方便地创建单例
    /// </summary>
    /// <typeparam name="T">约束为继承此类的类型</typeparam>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T> 
    {
        private static T _instance;
        public static T Instance => _instance;
        /// <summary>
        /// 获取单例是否已创建的属性
        /// </summary>
        public static bool IsInitialized => _instance is not null;

        protected virtual void Awake()
        {
            if (_instance is not null && _instance != this)
            {
                Destroy(gameObject); // 当单例已经被创建且不是this的时候，删除掉多余的单例
            }
            else
            {
                _instance = this as T;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null; // 当Destroy()被执行的时候，销毁单例
        }
    }
}