namespace GCFramework.Singleton
{
    public class SingletonNoMono<T> where T : new()
    {
        private static readonly object lockObj = typeof(T);
        private static T _instance;

        public static T Ins
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockObj)
                    {
                        _instance ??= new T();
                    }
                }
                return _instance;
            }
        }
    }
}