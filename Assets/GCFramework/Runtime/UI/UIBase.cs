using Sirenix.OdinInspector;
using UnityEngine;

namespace GCFramework.Runtime.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIBase : MonoBehaviour
    {
        #region Inspector Editor

        [ShowIf("HadAnimation")]
        public string uiShowAnimName;
        [ShowIf("HadAnimation")]
        public string uiHideAnimName;

        #endregion
        
        #region Components

        protected Animation uiAnim;
        protected CanvasGroup canvasGroup;

        #endregion

        #region Property Variables

        /// <summary>
        /// 是否初始化了？
        /// </summary>
        public bool IsInitialized { get; protected set; }
        /// <summary>
        /// 是否可交互了？
        /// </summary>
        public bool Interactable { get; protected set; }

        /// <summary>
        /// 是否动画过渡中
        /// </summary>
        public bool IsTransitioning { get; set; }

        #endregion

        #region System Methods

        protected virtual void Awake()
        {
            uiAnim = GetComponent<Animation>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Update()
        {
            if (!IsInitialized)
                return;
        }

        #endregion

        #region Show & Hide

        public virtual void Show()
        {
            if (HadAnimation() && IsHadAnimClip(uiShowAnimName)) uiAnim.Play(uiShowAnimName);
        }

        public virtual void Hide()
        {
            if (HadAnimation() && IsHadAnimClip(uiHideAnimName)) uiAnim.Play(uiHideAnimName);
        }

        #endregion

        #region UI的生命周期

        /// <summary>
        /// 当UI被生成时调用，仅调用一次
        /// </summary>
        public virtual void OnInit()
        {
            IsInitialized = true;
            Interactable = false;
        }

        /// <summary>
        /// 当UI被显示前调用
        /// </summary>
        public virtual void OnShow()
        {
        }

        /// <summary>
        /// 当UI被隐藏后调用
        /// </summary>
        public virtual void OnHide()
        {
        }
        
        public virtual void OnDestroy()
        {
        }

        #endregion

        #region Public

        /// <summary>
        /// 设置该 UI 页面是否可交互
        /// </summary>
        /// <param itemName="interactable"></param>
        public void SetInteractable(bool interactable)
        {
            Interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }

        #endregion

        #region Animation

        public bool HadAnimation()
        {
            return (uiAnim && uiAnim.GetClipCount() > 0) ||
                   (GetComponent<Animation>() && GetComponent<Animation>().GetClipCount() > 0);
        }

        public bool IsHadAnimClip(string animName)
        {
            return HadAnimation() && uiAnim.GetClip(animName) != null;
        }

        public bool IsPlaying()
        {
            return HadAnimation() && uiAnim.isPlaying;
        }

        public bool IsPlaying(string animState)
        {
            return HadAnimation() && uiAnim.IsPlaying(animState);
        }
        
        public float GetUIAnimProgress(string animState)
        {
            if (!HadAnimation()) return 0f;
            return uiAnim[animState].normalizedTime;
        }

        public bool IsShowEnd()
        {
            if (!HadAnimation())
                return true;
            return uiAnim.IsPlaying(uiShowAnimName) && uiAnim[uiShowAnimName].normalizedTime > 0.8f;
        }

        public bool IsHideEnd()
        {
            if (!HadAnimation())
                return true;
            return uiAnim.IsPlaying(uiHideAnimName) && uiAnim[uiHideAnimName].normalizedTime > 0.8f;
        }

        #endregion
        
    }
}