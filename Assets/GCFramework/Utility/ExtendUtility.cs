using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GCFramework.Utility
{
    /// <summary>
    /// 扩展工具
    /// </summary>
    public static class ExtendUtility
    {
        public static T SafeExecute<T>(Func<T> func, bool enableLog)
        {
            try
            {
                return func.Invoke();
            }
            catch (Exception e)
            {
                if(enableLog)
                    Debug.LogError($"执行出错。{e}");
            }

            return default;
        }

        public static List<Dictionary<string, object>> GetJsonTableData(string json, string sheetName)
        {
            Dictionary<string, List<Dictionary<string, object>>> jsonData = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(json);
            if (!jsonData.TryGetValue(sheetName, out var tableData))
            {
#if UNITY_EDITOR
                Debug.LogError($"获取JsonData失败！");
#endif
                return null;
            }

            return tableData;

        }

        #region Layer

        public static bool IsInLayer(this GameObject gameObject, LayerMask layers)
        {
            return (layers & (1 << gameObject.layer)) != 0;
        }

        #endregion
        
        /// <summary>
        /// 必须添加的组件
        /// </summary>
        /// <param itemName="go"></param>
        /// <param itemName="component"></param>
        /// <typeparam itemName="T"></typeparam>
        /// <returns></returns>
        public static bool RequireComponent<T>(this GameObject go, ref T component) where T : Component
        {
            if (!go)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"不能给空对象添加组件, gameObject为{go}");
#endif
                return false;
            }

            if (!go.TryGetComponent(out component))
            {
                // 强制查找，当前节点没有，就往下找
                component ??= go.GetComponentInChildren<T>();
                if (component == null && !typeof(T).IsAbstract) component = go.AddComponent<T>();
                if (component != null) return true;
                
#if UNITY_EDITOR
                Debug.LogWarning($"{go.name}的组件未找到或不存在, component为{typeof(T).Name}");
#endif
                return false;
            }

            return true;
        }
        
        public static bool RequireComponent<T>(this Component behaviour, ref T component) where T : Component
        {
            return RequireComponent<T>(behaviour.gameObject, ref component);
        }

        /// <summary>
        /// List的扩展方法，尝试添加数据
        /// </summary>
        /// <param itemName="list"></param>
        /// <param itemName="value"></param>
        /// <typeparam itemName="T"></typeparam>
        /// <returns></returns>
        public static bool TryAdd<T>(this List<T> list, T value)
        {
            if (list.Contains(value))
            {
#if UNITY_EDITOR
                Debug.LogError($"不能向表内添加同样元素！");
#endif
                return false;
            }
            list.Add(value);
            return true;
        }

        /// <summary>
        /// 以x,z轴为平面
        /// </summary>
        /// <param itemName="vec"></param>
        /// <returns></returns>
        public static Vector2 Vec3To2D(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        /// <summary>
        /// 以x,y轴为平面
        /// </summary>
        /// <param itemName="vec"></param>
        /// <returns></returns>
        public static Vector3 Vec2To3D(this Vector2 vec)
        {
            return new Vector3(vec.x, 0, vec.y);
        }
        
    }
}