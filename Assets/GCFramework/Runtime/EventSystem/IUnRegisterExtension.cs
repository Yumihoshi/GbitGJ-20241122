using System;
using GCFramework.Runtime.MessageCenter;
using UnityEngine;

namespace GCFramework.Runtime.EventSystem
{
    public static class IUnRegisterExtension
    {
        public static IUnRegister UnRegisterWhenGameObjectDestroyed<T>(this IUnRegister unRegister, T component)
            where T : Component
        {
            if (component == null)
            {
                throw new NullReferenceException("无法给空组件上的对象添加脚本！");
            }
            var trigger = component.gameObject.GetOrAddComponent<UnRegisterOnDestroyTrigger>();
            
            trigger.AddUnRegister(unRegister);
            return unRegister;
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            
            if (!component)
                component = gameObject.AddComponent<T>();
            
            return component;
        }
    }
}