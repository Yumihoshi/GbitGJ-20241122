using UnityEngine;

namespace GCFramework.Utility.ObjectPool
{
    /// <summary>
    /// 可对象池化
    /// </summary>
    public abstract class Poolable : MonoBehaviour
    {
        /// <summary>
        /// 是否被使用中？
        /// </summary>
        public bool IsUsing { get; set; }

        protected virtual void OnDisable()
        {
            Return();
        }

        /// <summary>
        /// 当在对象池中创建
        /// </summary>
        public void OnCreate() { }

        /// <summary>
        /// 进入对象池之前
        /// </summary>
        public virtual void OnReturn()
        {
            IsUsing = false;
        }

        /// <summary>
        /// 出对象池之后
        /// </summary>
        public virtual void OnGet()
        {
            IsUsing = true;
        }
        
        /// <summary>
        /// 返回
        /// </summary>
        public void Return()
        {
            PoolManager.Ins.ReturnToPool(this);
        }
        
        /// <summary>
        /// 销毁
        /// </summary>
        public void Release()
        {
            // 从对象池中注销管理
            Destroy(gameObject);
        }
    }
}