using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GCFramework.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GCFramework.Runtime.UI
{
    [RequireComponent(typeof(Canvas)
                    , typeof(CanvasScaler)
                    , typeof(GraphicRaycaster))]
    [RequireComponent(typeof(UIResManager))]
    public class UIManager : SingletonMono<UIManager>
    {
        #region Inspector Editor
        
        public UIBase initUI;

        #endregion

        #region Components

        public UIResManager UIResManager { get; private set; }

        #endregion

        #region Property Variables

        /// <summary>
        /// 常用 UIRoot
        /// </summary>
        public Transform UIRoot { get; private set; }
        /// <summary>
        /// 弹窗 UIRoot
        /// </summary>
        public Transform PopupUIRoot { get; private set; }
        /// <summary>
        /// 当前显示在最前面的UI
        /// </summary>
        [ShowInInspector, ReadOnly]
        public UIBase CurrentInDisplayUI { get; private set; }

        private bool _isPanelTransitioning;
        /// <summary>
        /// 是否处于界面跳转
        /// </summary>
        public bool IsPanelTransitioning
        {
            get => _isPanelTransitioning;
            private set
            {
                _isPanelTransitioning = value;
                Debug.Log($"当前UI操作区处于: {(!_isPanelTransitioning ? "禁用" : "激活")}");
            } 
        }

        #endregion

        #region Hide Variables

        /// <summary>
        /// UI修改回调，在UI打开前调用，OnShow()之前
        /// </summary>
        private Action<UIBase> _uiModificationAction;

        #endregion

        #region UI 数据结构

        /// <summary>
        /// UI栈
        /// </summary>
        private readonly Stack<UIBase> _uiStack = new Stack<UIBase>();
        
        /// <summary>
        /// 存放隐藏的UI，便于管理
        /// </summary>
        private readonly List<UIBase> _uiHideList = new List<UIBase>();

        #endregion

        #region System Methods

        protected override void Awake()
        {
            base.Awake();

            UIRoot = transform;
            PopupUIRoot = transform.Find("PopupWinCanvas");
            if (PopupUIRoot == null)
            {
                PopupUIRoot = new GameObject("PopupUIRoot", typeof(Canvas), typeof(GraphicRaycaster)).transform;
                PopupUIRoot.SetParent(transform);
#if UNITY_EDITOR
                Debug.LogWarning("未创建PopupWinCanvas");
#endif
            }
            UIResManager = GetComponent<UIResManager>();
            if (UIResManager == null)
                Debug.LogError("UIResManager未挂载！");
        }

        private void Start()
        {
            if (initUI) SetUI(initUI).Forget();
        }

        #endregion

        #region Show & Hide

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        /// <summary>
        /// 在顶部显示一个UI
        /// </summary>
        /// <param itemName="uiName">ui的名称</param>
        /// <param itemName="uiModification">可在此处对UI进行修改</param>
        /// <param itemName="onUIShowEnd">OnUI显示结束的回调</param>
        public async UniTask<UIBase> ShowUI(string uiName, Action<UIBase> uiModification = null, Action<UIBase> onUIShowEnd = null)
        {
            if (string.IsNullOrEmpty(uiName)
               || IsPanelTransitioning)
                return null;

            UIInfo uiInfo = UIResManager.GetUIInfo(uiName);
            _uiModificationAction = uiModification;

            if (uiInfo.uiType == UIType.Panel)
            {
                await HandlePanelJump(uiInfo, onUIShowEnd); // 处理界面跳转
            }
            else
            {
                if (CurrentInDisplayUI 
                    && uiInfo.uiType == UIType.Window 
                    && uiInfo.uiName == CurrentInDisplayUI.GetName())
                {
                    if (CurrentInDisplayUI.IsPlaying(CurrentInDisplayUI.uiHideAnimName))
                        _cancellationTokenSource.Cancel(); // 打断 窗口UI隐藏
                    return null;
                }
                
                UIBase ui = GetUIFromRes(uiName);
                PushUIToStack(ui);
                CurrentInDisplayUI = ui;

                await HandleUIShow(CurrentInDisplayUI);
                onUIShowEnd?.Invoke(ui);
                return ui;
            }

            return null;
        }

        /// <summary>
        /// 隐藏顶部UI
        /// </summary>
        public async void HideTopUI()
        {
            if (CurrentInDisplayUI == null 
                || _uiStack.Count == 0 
                || CurrentInDisplayUI.IsPlaying(CurrentInDisplayUI.uiHideAnimName)
                || IsPanelTransitioning)
                return;
            
            // 如果是关闭界面，判断是否是栈底界面，是则显示指定窗口，否则关闭界面
            if (CurrentInDisplayUI.GetUIType() == UIType.Panel)
            {
                // UIInfo targetUIInfo = UIResManager.GetUIInfo(CurrentInDisplayUI.GetName());
                // HandlePanelJump(targetUIInfo).Forget();
                if (_uiStack.Count == 1)
                {
                    Debug.Log($"打开界面 {CurrentInDisplayUI.GetName()} 的预设打开窗口");
                }
                else
                {
                    // PopUIFromStack();
                    HandleUIHide(CurrentInDisplayUI).Forget();
                }
            }
            else
            {
                // 关闭处于最上层的UI
                if (CurrentInDisplayUI.GetUIType() == UIType.Window && CurrentInDisplayUI.IsPlaying(CurrentInDisplayUI.uiHideAnimName))
                {
                    _cancellationTokenSource.Cancel();
                    return;
                }
                
                // 弹出UI栈
                UIBase ui = GetTopUI();
                HandleUIHide(ui).Forget();
            }
        }

        public async UniTask SetUI(string uiName, bool animate)
        {
            if (string.IsNullOrEmpty(uiName))
            {
#if UNITY_EDITOR
                Debug.LogError("传入的uiName为 NULL !");
#endif
                return;
            }
            
            UIBase ui = GetUIFromRes(uiName);
            await SetUI(ui, animate);
        }
        
        public async UniTask SetUI(UIBase ui, bool animate = false)
        {
            if (ui == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"无法将一个为 null 的UI进行设置");
#endif
                return;
            }
            
            if (ui.GetUIType() != UIType.Panel)
                return;
            
            // ClearAllUI();
            
            if (!ui.gameObject.activeSelf)
            {
                ui.OnShow();
                ui.gameObject.SetActive(true);
            }

            CurrentInDisplayUI = ui;
            CurrentInDisplayUI.OnInit();
            PushUIToStack(CurrentInDisplayUI);
            if (animate)
            {
                CurrentInDisplayUI.Show();
                CurrentInDisplayUI.IsTransitioning = true;
                await UniTask.WaitUntil(CurrentInDisplayUI.IsShowEnd);
                CurrentInDisplayUI.IsTransitioning = false;
            }
            CurrentInDisplayUI.SetInteractable(true);
        }

        public void ForceHideUI(UIBase ui)
        {
            if (ui == null)
            {
#if UNITY_EDITOR
                Debug.LogError("无法强制关闭一个NULL UI !");
#endif
                return;
            }
            
            if (GetTopUI() == ui)
            {
                PopUIFromStack();
                AddToHideList(ui);
            }
        }

        public void ClearAllUI()
        {
            while (_uiStack.Count > 0)
            {
                UIBase ui = _uiStack.Pop();
                ui.transform.SetAsFirstSibling();
                _uiHideList.Add(ui);
                ui.gameObject.SetActive(false);
                ui.SetInteractable(false);
            }
            _uiStack.Clear();
            CurrentInDisplayUI = null;
        }

        #endregion

        #region UI Stack

        public bool IsInUIStack(string uiName)
        {
            foreach (var ui in _uiStack)
            {
                if (ui.GetName() == uiName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取顶部UI
        /// </summary>
        /// <returns></returns>
        public UIBase GetTopUI()
        {
            if (_uiStack.Count == 0)
                return null;
            return _uiStack.Peek();
        }
        
        /// <summary>
        /// 将UI放入栈中
        /// </summary>
        /// <param itemName="ui"></param>
        private void PushUIToStack(UIBase ui)
        {
            GetTopUI()?.SetInteractable(false);
            _uiStack.Push(ui);
        }

        /// <summary>
        /// 从栈中弹出UI
        /// </summary>
        /// <returns></returns>
        private UIBase PopUIFromStack()
        {
            UIBase ui = _uiStack.Pop();
            ui.SetInteractable(false);
            GetTopUI()?.SetInteractable(true);
            return ui;
        }

        #endregion

        #region UI Hide List

        private void AddToHideList(UIBase ui)
        {
            if (ui == null)
                return;

            ui.transform.SetAsFirstSibling();
            ui.SetInteractable(false);
            if (ui.gameObject.activeSelf)
                ui.gameObject.SetActive(false);
            ui.OnHide();
            if (!_uiHideList.Contains(ui)) _uiHideList.Add(ui);
        }

        private UIBase RemoveFromHideList(UIBase ui)
        {
            if (ui == null)
                return null;

            if (_uiHideList.Contains(ui))
            {
                ui.transform.SetAsLastSibling();
                if (!ui.gameObject.activeSelf)
                {
                    ui.gameObject.SetActive(true);
                    // 在UI OnShow函数调用前，进行修改。
                    _uiModificationAction?.Invoke(ui);
                    ui.OnShow();
                }

                _uiHideList.Remove(ui);
            }
            // else
            // {
            //     ui.gameObject.SetActive(true);
            //     _uiModificationAction?.Invoke(ui);
            //     ui.OnShow();
            // }
            return ui;
        }

        public UIBase GetFromHideList(string uiName)
        {
            var ui = _uiHideList.Find(x => x.GetName() == uiName);
            if (ui == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"UI {uiName} 不在HideList中");
#endif
            }
            
            return RemoveFromHideList(ui);
        }
        
        public void RemoveAndDestroyFromHideList(string uiName)
        {
            List<UIBase> list = new List<UIBase>();
            for (int i = _uiHideList.Count - 1; i >=0 ; i--)
            {
                if (_uiHideList[i] && _uiHideList[i].GetName() == uiName)
                {
                    list.Add(_uiHideList[i]);
                    _uiHideList.RemoveAt(i);
                }
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Destroy(list[i]);
            }
        }

        public void RemoveAndDestroyFromHideList(UIBase ui)
        {
            UIBase hideUI = _uiHideList.Find(x => x == ui);
            if (hideUI)
            {
                _uiHideList.Remove(hideUI);
                Destroy(hideUI);
            }
        }
        
        #endregion

        /// <summary>
        /// 从资源中获取UI
        /// </summary>
        /// <param itemName="uiName"></param>
        /// <returns></returns>
        private UIBase GetUIFromRes(string uiName)
        {
            UIBase ui = GetFromHideList(uiName);
            if (ui != null)
                return ui;
            ui = UIResManager.LoadUI(uiName);
            // ui.transform.SetParent(UIRoot);
            var rect = ui.GetComponent<RectTransform>();
            if (ui.GetUIType() != UIType.PopupWindow) rect.SetParent(UIRoot);
            else rect.SetParent(PopupUIRoot);
            // rect.localPosition = Vector3.zero;
            rect.anchoredPosition = Vector2.zero;
            // rect.sizeDelta = Vector2.zero;
            
            if (rect.localScale != Vector3.one)
                rect.localScale = Vector3.one;
            
            // 加载出来时需要调用OnShow函数
            _uiModificationAction?.Invoke(ui);
            ui.OnShow();
            
            return ui;
        }
        
        /// <summary>
        /// 等待UI显示动画完毕
        /// </summary>
        /// <param itemName="ui"></param>
        /// <returns></returns>
        private async UniTask HandleUIShow(UIBase ui)
        {
            // 从HideList移出
            RemoveFromHideList(ui);
            
            ui.Show();
            ui.IsTransitioning = true;
            await UniTask.WaitUntil(ui.IsShowEnd, cancellationToken: _cancellationTokenSource.Token);

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                HandleUICancelLogic(ui, true);
                return;
            }

            // 防止UI打开了，但是不在栈中
            if (ui.GetUIType() == UIType.Window && !IsInUIStack(ui.GetName()))
                PushUIToStack(ui);
            
            ui.IsTransitioning = false;
            ui.SetInteractable(true);
        }

        /// <summary>
        /// 等待UI隐藏动画完毕
        /// </summary>
        /// <param itemName="ui"></param>
        /// <returns></returns>
        private async UniTask HandleUIHide(UIBase ui)
        {
            ui.Hide();
            ui.IsTransitioning = true;
            await UniTask.WaitUntil(ui.IsHideEnd, cancellationToken: _cancellationTokenSource.Token);

            // 当被打断
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                HandleUICancelLogic(ui, false);
                return;
            }

            ui.IsTransitioning = false;
            
            AddToHideList(ui); // 放入隐藏UI列表

            // 防止UI被关闭了，但仍然还是栈中
            if (ui.GetUIType() == UIType.Window && IsInUIStack(ui.GetName()))
                PopUIFromStack(); 
            CurrentInDisplayUI = GetTopUI();
        }

        /// <summary>
        /// 处理界面之间跳转
        /// </summary>
        private async UniTask HandlePanelJump(UIInfo targetUIInfo, Action<UIBase> onUIShowEnd = null)
        {
            IsPanelTransitioning = true;
            //当前在哪？
            // 处理当前显示
            // 某个界面处，先关闭界面，黑幕过渡，
            // 某个窗口，黑幕过渡
            if (CurrentInDisplayUI && CurrentInDisplayUI.GetUIType() == UIType.Panel)
            {
                CurrentInDisplayUI.Hide();
                
                if (CurrentInDisplayUI.HadAnimation() && CurrentInDisplayUI.IsHadAnimClip(CurrentInDisplayUI.uiHideAnimName))
                {
                    CurrentInDisplayUI.IsTransitioning = true;
                    await UniTask.WaitUntil(() => CurrentInDisplayUI.IsPlaying()
                                                  && CurrentInDisplayUI.GetUIAnimProgress(CurrentInDisplayUI.uiHideAnimName) > 0.6f);
                }
            }
            // 过渡阶段
            // 黑幕过渡渐入
            await TransitionManager.Ins.EnterTransition(TransitionManager.TransitionEffect.FadeInOut_Panel);
            
            // 处理显示界面
            // 加载一个UI界面
            UIBase uiPanel = null;
            // 界面只有一个，如果已经在栈中了，说明是要快速回到之前界面
            if (IsInUIStack(targetUIInfo.uiName))
            {
                CurrentInDisplayUI.IsTransitioning = false;
                if (CurrentInDisplayUI.gameObject.activeSelf)
                    CurrentInDisplayUI.gameObject.SetActive(false);

                CurrentInDisplayUI.transform.SetAsFirstSibling();
                CurrentInDisplayUI.OnHide();
                _uiHideList.Add(CurrentInDisplayUI);
                PopUIFromStack();
                
                // 跳转之前界面前，判断其前面是否有窗口，如果有关闭它们
                while (GetTopUI().GetName() != targetUIInfo.uiName)
                {
                    GetTopUI().ForceHideSelf();
                    await UniTask.Yield();
                }

                uiPanel = GetTopUI();
                _uiModificationAction?.Invoke(uiPanel);
                uiPanel.SetInteractable(true);
                CurrentInDisplayUI = uiPanel;
                await TransitionManager.Ins.ExitTransition(TransitionManager.TransitionEffect.FadeInOut_Panel, 0.15f);
                IsPanelTransitioning = false;
            }
            else // 不是，则只是跳转到一个新界面而已
            {
                if (CurrentInDisplayUI)
                    CurrentInDisplayUI.IsTransitioning = false;
                uiPanel = GetUIFromRes(targetUIInfo.uiName);
                PushUIToStack(uiPanel);
                CurrentInDisplayUI = uiPanel;
                await TransitionManager.Ins.ExitTransition(TransitionManager.TransitionEffect.FadeInOut_Panel, 0.15f);
                
                await HandleUIShow(CurrentInDisplayUI);
            
                onUIShowEnd?.Invoke(CurrentInDisplayUI);
                IsPanelTransitioning = false;
            }

            // CurrentInDisplayUI = uiPanel;
            // await TransitionManager.Ins.ExitTransition(TransitionManager.TransitionEffect.FadeInOut_Panel, 0.15f);
            //
            // // 在显示前，对UI进行修改
            // uiModification?.Invoke(CurrentInDisplayUI);
            //
            // await HandleUIShow(CurrentInDisplayUI);
            //
            // onUIShowEnd?.Invoke(CurrentInDisplayUI);
            // IsPanelTransitioning = false;
        }

        /// <summary>
        /// 处理 UI切换 打断逻辑
        /// </summary>
        /// <param itemName="ui"></param>
        /// <param itemName="cancelShow"></param>
        private void HandleUICancelLogic(UIBase ui, bool cancelShow)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            if (cancelShow)
            {
                HandleUIHide(ui).Forget();
            }
            else
            {
                HandleUIShow(ui).Forget();
            }
        }
    }
}