using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GCFramework.Runtime.UI
{
    [CreateAssetMenu(menuName = "DataModel/UI配置数据", fileName = "UIConfigData")]
    public class UIConfigData : ScriptableObject
    {
        [Multiline(3), ReadOnly, HideLabel]
        public string description = "UI预制件需要关闭放置，防止实例化后立即调用OnEnable导致UI函数的调用时机出现问题！";
        public string mainLoadPath = "UI";
        [Title("UIName的命名空间")]
        // ReSharper disable once InconsistentNaming
        public string uiName_NameSpace = "Game.UI";

        [Title("UI界面")]
        public List<UIInfo> panelList;
        [Title("UI窗口")]
        public List<UIInfo> windowList;
        [Title("UI弹窗")]
        public List<UIInfo> popupWindowList;

        [Button("从UIName获取数据添加UI")]
        public void LoadUIData()
        {
            Type uiPanelNameClass = Type.GetType(uiName_NameSpace + ".UIPanelName");
            Type uiWindowNameClass = Type.GetType(uiName_NameSpace + ".UIWindowName");
            Type uiPopupWindowNameClass = Type.GetType(uiName_NameSpace + ".UIPopupWinName");

            if (uiPanelNameClass != null)
            {
                var fieldInfos = uiPanelNameClass.GetFields();

                foreach (var fileInfo in fieldInfos)
                {
                    // 是否为静态变量
                    if (fileInfo.IsPublic && fileInfo.IsStatic)
                    {
                        var panelName = panelList.Find(x => x.uiName == fileInfo.GetValue(null).ToString());
                        if (panelName != null)
                            continue;
                        
                        panelList.Add(new UIInfo
                        {
                            uiName = fileInfo.GetValue(null).ToString(),
                            uiType = UIType.Panel,
                            isCache = false
                        });
                    }
                }
            }

            if (uiWindowNameClass != null)
            {
                var fieldInfos = uiWindowNameClass.GetFields();

                foreach (var fileInfo in fieldInfos)
                {
                    if (fileInfo.IsPublic && fileInfo.IsStatic)
                    {
                        var windowName = windowList.Find(x => x.uiName == fileInfo.GetValue(null).ToString());
                        if (windowName != null)
                            continue;
                        
                        windowList.Add(new UIInfo
                        {
                            uiName = fileInfo.GetValue(null).ToString(),
                            uiType = UIType.Window,
                            isCache = false
                        });
                    }
                }
            }
            
            if (uiPopupWindowNameClass != null)
            {
                var fieldInfos = uiPopupWindowNameClass.GetFields();

                foreach (var fileInfo in fieldInfos)
                {
                    if (fileInfo.IsPublic && fileInfo.IsStatic)
                    {
                        var popupWindowName = popupWindowList.Find(x => x.uiName == fileInfo.GetValue(null).ToString());
                        if (popupWindowName != null)
                            continue;
                        
                        popupWindowList.Add(new UIInfo
                        {
                            uiName = fileInfo.GetValue(null).ToString(),
                            uiType = UIType.PopupWindow,
                            isCache = false
                        });
                    }
                }
            }

            if (uiWindowNameClass == null || uiPanelNameClass == null || uiPopupWindowNameClass == null)
            {
                Debug.LogWarning($"检查UIName的命名空间是否无误？并且各个静态类名是否错误！");
                return;
            }
            Debug.Log("[UIConfigData] 加载UIInfo成功！");
        }

        // [Button("测试")]
        // public void GetAssembly()
        // {
        //     Type staticClass = Type.GetType("");
        //
        //     
        // }

        public string GetLoadPath(UIType uiType)
        {
            return uiType switch
            {
                UIType.Panel => mainLoadPath + "/Panel",
                UIType.Window => mainLoadPath + "/Window",
                UIType.PopupWindow => mainLoadPath + "/PopupWindow",
                _ => throw new ArgumentOutOfRangeException(nameof(uiType), uiType, null)
            };
        }
    }

    public enum UIType
    {
        Panel, 
        Window,
        PopupWindow
    }

    [System.Serializable]
    public class UIInfo
    {
        /// <summary>
        /// UI预制件
        /// </summary>
        public string uiName;
        /// <summary>
        /// UI类型
        /// </summary>
        public UIType uiType;
        // /// <summary>
        // /// UI层级
        // /// </summary>
        // public UILayerManager.UILayer uiLayer;
        // /// <summary>
        // /// 优先级
        // /// </summary>
        // public int priority;
        /// <summary>
        /// 是否缓存
        /// </summary>
        public bool isCache;
        // /// <summary>
        // /// 是否黑幕过渡
        // /// </summary>
        // public bool isTransition;
    }
}