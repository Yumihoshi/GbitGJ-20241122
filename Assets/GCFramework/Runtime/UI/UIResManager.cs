using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GCFramework.Runtime.UI
{
    public class UIResManager : MonoBehaviour
    {
        public UIConfigData configData;

        /// <summary>
        /// 缓存UI的原型
        /// </summary>
        [ShowInInspector, ReadOnly]
        private readonly Dictionary<string, UIBase> _uiCaches = new Dictionary<string, UIBase>();

        public UIBase LoadUI(string uiName)
        {
            UIBase ui = null;
            if (_uiCaches.TryGetValue(uiName, out UIBase uiProtoType))
            {
                ui = Instantiate(uiProtoType);
            }
            else
            {
                var info = GetUIInfo(uiName);
                // TODO:: Resources加载，可更换其他
                var uiRes = Resources.Load<UIBase>(Path.Combine(configData.GetLoadPath(info.uiType), uiName));
                if (uiRes == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"加载UI {uiName} 时错误！");
                    return null;
#endif
                }
                if (info.isCache)
                    _uiCaches.TryAdd(uiName, uiRes);
                ui = Instantiate(uiRes);
            }
            
            if (ui) 
                ui.OnInit();
            return ui;
        }

        public void ReleaseUI(UIBase ui)
        {
            Destroy(ui.gameObject);
        }

        public UIInfo GetUIInfo(string uiName)
        {
            UIInfo uiInfo = null;
            uiInfo = configData.panelList.Find(x => x.uiName == uiName);
            if (uiInfo != null)
                return uiInfo;
            uiInfo = configData.windowList.Find(x => x.uiName == uiName);
            if (uiInfo != null)
                return uiInfo;
            uiInfo = configData.popupWindowList.Find(x => x.uiName == uiName);

            if (uiInfo == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"未找到 UI Name 为{uiName} 的 UI Info");
#endif
            }
            
            return uiInfo;
        }
    }
}