using System;
using TMPro;
using UnityEngine.UI;

namespace GCFramework.Runtime.UI.UISub.ModalWindow
{
    public class UITipsPopupWindow : UIPopupWinBase, IWinClickAnywhere, IWinOptional
    {
        public static string TipsWindow = "TipsWindow";
        
        // 标题
        public TextMeshProUGUI titleText;
        
        // 内容
        public TextMeshProUGUI contentText;
        
        // 点击按钮
        public Button confirmBtn;
        public Button cancelBtn;
        public Button alternateBtn;

        protected override void Awake()
        {
            base.Awake();
            
            InitUIComponent();
        }

        private void OnEnable()
        {
            confirmBtn.onClick.AddListener(Confirm);
            cancelBtn.onClick.AddListener(Cancel);
            alternateBtn.onClick.AddListener(Alternate);
        }

        private void OnDisable()
        {
            confirmBtn.onClick.RemoveListener(Confirm);
            cancelBtn.onClick.RemoveListener(Cancel);
            alternateBtn.onClick.RemoveListener(Alternate);
        }

        #region Public Methods

        /// <summary>
        /// 设置模态提示弹窗
        /// </summary>
        /// <param itemName="content">弹窗文本内容</param>
        /// <param itemName="alternation">可选项的按钮</param>
        /// <param itemName="title">窗口标题</param>
        public void SetTips(string content, string alternation = "", string title = "Tips Window")
        {
            contentText.text = content;
            if (!string.IsNullOrEmpty(alternation))
            {
                alternateBtn.GetComponentInChildren<TextMeshProUGUI>().text = alternation;
                alternateBtn.gameObject.SetActive(true);
            }
            else
            {
                alternateBtn.gameObject.SetActive(false);
            }

            titleText.text = title;
        }

        #endregion

        private void InitUIComponent()
        {
            titleText = transform.Find("Header/title").GetComponent<TextMeshProUGUI>();
            contentText = transform.Find("Content/content").GetComponent<TextMeshProUGUI>();
            confirmBtn = transform.Find("Footer/confirm_btn").GetComponent<Button>();
            cancelBtn = transform.Find("Footer/cancel_btn").GetComponent<Button>();
            alternateBtn = transform.Find("Footer/alternate_btn").GetComponent<Button>();
        }

        public void ClickAnyWhereHide()
        {
            this.HideSelf();
        }

        public Action<int> OptionCallback { get; set; }

        public void Confirm()
        {
            OptionCallback?.Invoke(1);
            ClearCallback();
            this.HideSelf();
        }

        public void Cancel()
        {
            OptionCallback?.Invoke(2);
            ClearCallback();
            this.HideSelf();
        }

        public void Alternate()
        {
            OptionCallback?.Invoke(3);
            ClearCallback();
            this.HideSelf();
        }

        private void ClearCallback() => OptionCallback = null;
    }
}