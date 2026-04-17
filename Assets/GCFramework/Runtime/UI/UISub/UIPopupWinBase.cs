using System;

namespace GCFramework.Runtime.UI.UISub
{
    /// <summary>
    /// 点击任意处关闭窗口
    /// </summary>
    public interface IWinClickAnywhere
    {
        public void ClickAnyWhereHide();
    }

    /// <summary>
    /// 具有确认与取消，或者其他的 窗口
    /// </summary>
    public interface IWinOptional
    {
        public Action<int> OptionCallback { get; }
        /// <summary>
        /// 确认
        /// </summary>
        public void Confirm();
        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel();
        /// <summary>
        /// 可变动选项
        /// </summary>
        public void Alternate();
    }
    
    public class UIPopupWinBase : UIBase
    {
        
    }
}