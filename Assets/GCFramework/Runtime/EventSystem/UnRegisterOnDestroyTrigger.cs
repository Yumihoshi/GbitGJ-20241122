using System.Collections.Generic;
using UnityEngine;

namespace GCFramework.Runtime.MessageCenter
{
    public class UnRegisterOnDestroyTrigger : MonoBehaviour
    {
        /// <summary>
        /// 用于存放注销委托类
        /// </summary>
        private readonly HashSet<IUnRegister> UnRegisters = new();

        public void AddUnRegister(IUnRegister unRegister) => UnRegisters.Add(unRegister);
        
        public void RemoveUnRegister(IUnRegister unRegister) => UnRegisters.Remove(unRegister);

        /// <summary>
        /// 当对象被禁用时，会注销对象上的类里注册的所有方法
        /// </summary>
        private void OnDisable()
        {
            Release();
        }

        /// <summary>
        /// 当对象被销毁，会注销该对象上类里注册的所有方法
        /// </summary>
        private void OnDestroy()
        {
            Release();
        }

        private void Release()
        {
            if(UnRegisters.Count == 0) return;
            foreach (var unRegister in UnRegisters)
            {
                unRegister.UnRegister();
            }
            
            UnRegisters.Clear();
        }
    }
}