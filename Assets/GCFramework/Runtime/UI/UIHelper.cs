using System;
using GCFramework.Runtime.UI.UISub;
using GCFramework.Runtime.UI.UISub.ModalWindow;
using UnityEngine;

namespace GCFramework.Runtime.UI
{
    #region UI Helper

    public static class UIHelper
    {
        #region UIBase

        /// <summary>
        /// 对于处于最顶部的UI，它可以关闭自己
        /// </summary>
        /// <param itemName="ui"></param>
        public static void HideSelf(this UIBase ui)
        {
            if (UIManager.Ins.GetTopUI() == ui)
            {
                UIManager.Ins.HideTopUI();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"尝试关闭不处于最顶部的UI: {ui.name}，这是不允许的！");
#endif
            }
        }

        /// <summary>
        /// UI 强制关闭自己，不会播放动画
        /// </summary>
        /// <param itemName="ui"></param>
        public static void ForceHideSelf(this UIBase ui)
        {
            UIManager.Ins.ForceHideUI(ui);
        }
        
        public static string GetName(this UIBase uiBase)
        {
            return uiBase.name.Replace("(Clone)", "");
        }

        public static UIType GetUIType(this UIBase uiBase)
        {
            return uiBase switch
            {
                UIPanelBase => UIType.Panel,
                UIWindowBase => UIType.Window,
                UIPopupWinBase => UIType.PopupWindow,
                _ => default
            };
        }

        #endregion

        public static UIBase OptionalCallback(this UIBase ui, Action<int> callback)
        {
            if (ui.TryGetComponent(out UITipsPopupWindow tipsPopupWindow))
            {
                tipsPopupWindow.OptionCallback = callback;
            }

            return ui;
        }
        
    }

    #endregion
}