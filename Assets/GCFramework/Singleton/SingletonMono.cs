using UnityEngine;

namespace GCFramework.Singleton
{
    public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        private static readonly object LockObject = typeof(T);
        protected static volatile T instance;

        public static T Ins
        {
            get
            {
                if (instance == null)
                {
                    // 进入锁定
                    lock (LockObject)
                    {
                        instance = FindObjectOfType<T>();
                        if (instance == null)
                        {
                            // 第二次检查，防止在获取锁之前已经有其他线程创建了实例
                            instance = new GameObject("Singleton Of " + typeof(T))
                                .AddComponent<T>();
                        }
                    }
                }

                return instance;
            }
        }
        
        [Header("是否作为全局单例")]
        public bool isGlobal = true;

        private void Register()
        {
            if (instance == null)
            {
                instance = (T)this;
            }
            else if(instance != this)
            {
#if UNITY_EDITOR
                Debug.Log($"{typeof(T)} 单例重复");
#endif
                Destroy(gameObject);
                return;
            }
            
            if(isGlobal)
                DontDestroyOnLoad(gameObject);
        }

        protected virtual void Awake() => Register();
        
        public static bool IsInitialized => instance != null;

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}