using System.Collections.Generic;
using System.Linq;
using GCFramework.Singleton;
using UnityEngine;

namespace GCFramework.Utility.ObjectPool
{
    public class PoolManager : SingletonMono<PoolManager>
    {
        // 对象池管理容器
        private readonly Dictionary<string, List<Poolable>> _objectPool = new Dictionary<string, List<Poolable>>();
        private readonly Dictionary<string, Transform> _poolObjRootDic = new Dictionary<string, Transform>();

        // 对象池根节点
        public Transform PoolRoot { get; private set; }
        public List<Poolable> preRegisterPoolObjList;
        
        public int preCacheCount = 10;

        public int poolMaxCount = 1000;

        protected override void Awake()
        {
            base.Awake();
            PoolRoot = new GameObject("Pool Root").transform;
            PoolRoot.SetParent(transform);
            InitPool();
        }

        public bool HadRoot(string poolKey)
        {
            return !string.IsNullOrEmpty(poolKey) && _poolObjRootDic.ContainsKey(poolKey);
        }

        public string GetKey(Poolable poolObj)
        {
            if (poolObj == null)
                return string.Empty;
            return poolObj.name.Replace("(Clone)", "");
        }

        private void InitPool()
        {
            // 创建根节点
            for (int i = 0; i < preRegisterPoolObjList.Count; i++)
            {
                var poolObj = preRegisterPoolObjList[i];
                if (!HadRoot(GetKey(poolObj))) 
                    CreateObjRoot(poolObj);
                _objectPool.Add(poolObj.name, new List<Poolable>());
                for (int j = 0; j < preCacheCount; j++)
                {
                    var objIns = CreateObject(poolObj);
                    objIns.OnReturn();
                    _objectPool[poolObj.name].Add(objIns);
                }
            }
        }

        private void CreateObjRoot(Poolable poolObj)
        {
            if (poolObj == null)
                return;
            
            var root = new GameObject($"{poolObj.gameObject.name} Root");
            root.transform.SetParent(PoolRoot);
            _poolObjRootDic.Add(poolObj.name, root.transform);
        }

        private Poolable CreateObject(Poolable objPrefab)
        {
            var objIns = Instantiate(objPrefab, _poolObjRootDic[objPrefab.name]);
            objIns.OnCreate();
            return objIns;
        }

        #region Return & Get & Return

        public void ReturnToPool(Poolable poolObj)
        {
            if (poolObj == null)
            {
                Debug.LogError("[ObjectPool] 放入了一个空引用！");
                return;
            }

            string key = GetKey(poolObj);
            if (_objectPool.TryGetValue(key, out var pool))
            {
                if (pool.Count < poolMaxCount)
                {
                    if (HadRoot(key) && poolObj.transform.parent != _poolObjRootDic[key])
                        poolObj.transform.SetParent(_poolObjRootDic[key]);
                    poolObj.OnReturn();
                    pool.Add(poolObj);
                }
                else
                {
                    poolObj.Release();
                }
            }
            else
            {
                // 没有注册过，则创建Root之后 再加入对象池
                if (!HadRoot(key)) CreateObjRoot(poolObj);
                _objectPool.Add(key, new List<Poolable>());
                var objIns = CreateObject(poolObj);
                objIns.OnReturn();
                _objectPool[key].Add(objIns);
            }
        }

        public T GetFromPool<T>(Poolable poolObj) where T : Poolable
        {
            if (poolObj == null)
            {
                Debug.LogError("[ObjectPool] 无法根据一个空引用获取对象！");
                return null;
            }

            string key = GetKey(poolObj);
            Poolable objIns = null;
            List<Poolable> pool;
            if (_objectPool.TryGetValue(key, out pool))
            {
                if (pool.Count == 0)
                {
                    objIns = CreateObject(poolObj);
                }
                else
                {
                    objIns = pool.First();
                    pool.Remove(objIns);
                }
            }
            else
            {
                if (!HadRoot(key)) CreateObjRoot(poolObj);
                _objectPool.Add(key, new List<Poolable>());
                objIns = CreateObject(poolObj);
            }

            if (objIns)
            {
                objIns.OnGet();
            }
            return (T)objIns;
        }

        public void Clear(Poolable poolObj)
        {
            if (poolObj == null)
                return;
            string key = GetKey(poolObj);
            if (_objectPool.TryGetValue(poolObj.name, out var pool))
            {
                for (int i = pool.Count - 1; i >= 0; i++)
                {
                    if (pool[i]) pool[i].Release();
                }

                _objectPool.Remove(poolObj.name);
            }
        }

        #endregion
    }
}