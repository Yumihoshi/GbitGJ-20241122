using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GCFramework.Runtime.UI.TransitionEffectUIs
{
    /// <summary>
    /// 过渡UI，默认渐变效果
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class TransitionUI_NormalFade : MonoBehaviour, ITransitionUI
    {
        public float duration = 0.3f;
        public Ease fadeInEase;
        public Ease fadeOutEase;
        
        
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public async UniTask Enter()
        {
            gameObject.SetActive(true);
            await _canvasGroup.DOFade(1f, duration).SetEase(fadeInEase).ToUniTask();
        }

        public async UniTask Exit()
        {
            await _canvasGroup.DOFade(0f, duration).SetEase(fadeOutEase).ToUniTask();
            gameObject.SetActive(false);
        }
    }
}