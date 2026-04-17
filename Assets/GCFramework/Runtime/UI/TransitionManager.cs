using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GCFramework.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GCFramework.Runtime.UI
{
    public interface ITransitionUI
    {
        public UniTask Enter();
        public UniTask Exit();
    }
    
    public class TransitionManager : SingletonMono<TransitionManager>
    {
        public enum TransitionEffect
        {
            FadeInOut_Scene,
            FadeInOut_Panel,
            FadeInOut_Custom
        }
        public Transform TransitionUIRoot { get; private set; }
        [ShowInInspector, ReadOnly]

        public List<ITransitionUI> transitionUis = new List<ITransitionUI>();

        protected override void Awake()
        {
            base.Awake();

            TransitionUIRoot = transform;
            transitionUis.AddRange(TransitionUIRoot.GetComponentsInChildren<ITransitionUI>(true));
        }

        public async UniTask EnterTransition(TransitionEffect effect)
        {
            if (transitionUis == null || transitionUis.Count == 0)
                return;
            
            switch (effect)
            {
                case TransitionEffect.FadeInOut_Scene:
                    await transitionUis[0].Enter();
                    break;
                case TransitionEffect.FadeInOut_Panel:
                    await transitionUis[1].Enter();
                    break;
                case TransitionEffect.FadeInOut_Custom:
                    await transitionUis[2].Enter();
                    break;
                default:
                    await transitionUis[0].Enter();
                    break;
            }
        }

        public async UniTask ExitTransition(TransitionEffect effect, float waitTime = 0.2f)
        {
            if (transitionUis == null || transitionUis.Count == 0)
                return;
            
            switch (effect)
            {
                case TransitionEffect.FadeInOut_Scene:
                    await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(waitTime), DelayType.UnscaledDeltaTime), 
                        transitionUis[0].Exit());
                    break;
                case TransitionEffect.FadeInOut_Panel:
                    await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(waitTime), DelayType.UnscaledDeltaTime), 
                        transitionUis[1].Exit());
                    break;
                case TransitionEffect.FadeInOut_Custom:
                    await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(waitTime), DelayType.UnscaledDeltaTime), 
                        transitionUis[2].Exit());
                    break;
                default:
                    await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(waitTime), DelayType.UnscaledDeltaTime), 
                        transitionUis[0].Exit());
                    break;
            }
        }
    }
}