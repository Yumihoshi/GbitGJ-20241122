// ********************************************************************************
// @author: Starry Sky
// @email: xiaoyuesun915@gmail.com
// @creationDate: 2024/11/26 14:11
// @version: 1.0
// @description:
// ********************************************************************************

using UnityEngine;

namespace Tsuki_Test.Enemy
{
    public enum HideType
    {
        NoHide,
        Once,
        Continuous
    }
    
    public interface IHideable
    {
        /// <summary>
        /// 是否处于伪装状态
        /// </summary>
        public bool IsHide { get; }

        /// <summary>
        /// 伪装类型，0 为暴露后不伪装，1 为暴露后伪装
        /// </summary>
        public HideType GetHideType();

        /// <summary>
        /// 解除当前怪物伪装
        /// </summary>
        public void RemoveHide();

        /// <summary>
        /// 解除整个链表的怪物伪装
        /// </summary>
        public void RemoveListEnemiesHide();
    }
}
