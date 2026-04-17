using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GCFramework.Runtime.UI
{
    /// <summary>
    /// 切换场景后，将UI界面进行替换
    /// </summary>
    public class UIPanelSetter : MonoBehaviour
    {
        public string panelReplaceWith;
        
        private void Start()
        {
            UIManager.Ins.ClearAllUI();
            UIManager.Ins.SetUI(panelReplaceWith, true).Forget();
        }
    }
}