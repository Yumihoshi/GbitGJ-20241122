// ********************************************************************************
// @author: Starry Sky
// @email: xiaoyuesun915@gmail.com
// @creationDate: 2024/11/24 11:11
// @version: 1.0
// @description:
// ********************************************************************************

using System;
using JetBrains.Annotations;
using Tsuki_Test.Common;
using UnityEngine;

namespace Tsuki_Test.Enemy
{
    /// <summary>
    /// 允许触发 QTE 的敌人
    /// </summary>
    public interface IQteable
    {
        /// <summary>
        /// 当前怪物节点
        /// </summary>
        public DoublyLinkedListNode<QteEnemy> CurrentNode { get; }
        
        /// <summary>
        /// QTE 可触发时回调事件
        /// </summary>
        [CanBeNull] public event Action OnQteableCallback;

        /// <summary>
        /// 获取是否可以触发 QTE
        /// </summary>
        /// <returns></returns>
        public bool GetQteable();

        /// <summary>
        /// 获取反弹向量
        /// </summary>
        /// <param name="player">玩家transform</param>
        /// <returns></returns>
        public Vector2 GetBounceVector(Transform player);

        /// <summary>
        /// 杀死整个链表的怪物
        /// </summary>
        public void KillListEnemies();

        /// <summary>
        /// Qte失败，给玩家造成伤害
        /// </summary>
        public void CauseDamage(int damage);
    }
}
