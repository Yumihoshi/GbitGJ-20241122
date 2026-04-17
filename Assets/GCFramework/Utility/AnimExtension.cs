using System;
using UnityEngine;

namespace GCFramework.Utility
{
    public static class AnimExtension
    {
        public static void PlayAnim(Animator animator, int animId, object animPara, int animIndex = 0, Action onCompleted = null)
        {
            SetAnimParas(animator, animId, animPara);
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(animIndex);
        }

        private static void SetAnimParas(Animator animator, int animId, object animPara = null)
        {
            if (animPara != null)
            {
                try
                {
                    Type type = animPara.GetType();

                    if (type == typeof(int))
                    {
                        animator.SetInteger(animId, (int)animPara);
                    }
                    else if(type == typeof(float))
                    {
                        animator.SetFloat(animId, (float)animPara);
                    }
                    else if (type == typeof(bool))
                    {
                        animator.SetBool(animId, (bool)animPara);
                    }
                }
                catch (Exception e)
                {
#if UNITY_EDITOR
                    Debug.LogError($"动画播放失败，类型转换失败！{e}");
#endif
                }
            }
            else
            {
                animator.SetTrigger(animId);
            }
        }
        
        public static float GetDuration(this Animator animator, int layerIdx = 0)
        {
            var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(layerIdx);
            return animatorStateInfo.normalizedTime;
        }

        public static bool IsName(this Animator animator, string name, int index = 0)
        {
            return animator.GetCurrentAnimatorStateInfo(index).IsName(name);
        }

        public static bool IsTag(this Animator animator, string tag, int index = 0)
        {
            return animator.GetCurrentAnimatorStateInfo(index).IsTag(tag);
        }

        public static bool IsCompleted(this Animator animator, int layerIdx)
        {
            return animator.GetCurrentAnimatorStateInfo(layerIdx).normalizedTime >= 1.0f && !animator.IsInTransition(layerIdx);
        }

        public static bool IsAnimCompletedByName(this Animator animator, string animHashName, int layerIdx = 0)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIdx);
            return stateInfo.IsName(animHashName) && stateInfo.normalizedTime >= 1.0f &&
                   !animator.IsInTransition(layerIdx);
        }

        public static bool IsAnimCompletedByTag(this Animator animator, string animTag, int layerIdx = 0)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIdx);
            return stateInfo.IsTag(animTag) && stateInfo.normalizedTime >= 1.0f &&
                   !animator.IsInTransition(layerIdx);
        }
    }
}