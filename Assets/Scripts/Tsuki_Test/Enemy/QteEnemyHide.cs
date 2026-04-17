// ********************************************************************************
// @author: Starry Sky
// @email: xiaoyuesun915@gmail.com
// @creationDate: 2024/11/28 20:11
// @version: 1.0
// @description:
// ********************************************************************************

using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tsuki_Test.Enemy
{
    /// <summary>
    /// Qte 敌人伪装处理
    /// </summary>
    public class QteEnemyHide : MonoBehaviour, IHideable
    {
        public QteEnemy QteEnemyComponent { get; private set; }
        // 伪装相关
        public bool IsHide
        {
            get => _isHide;
            private set
            {
                _isHide = value;
                OnHideStatusChanged?.Invoke(_isHide, hideType);
            }
        } // 是否伪装
        
        [CanBeNull] private event Action<bool, HideType> OnHideStatusChanged; // 隐藏时回调事件

        private bool _isHide;
        [Header("伪装")]
        public HideType hideType;        // 伪装类型 0 为暴露后不伪装，1 为暴露后伪装

        public float showTime = 3f;      // 解除伪装暴露时间
        
        private Color _originColor;
        private Color _hideColor;
        private SpriteRenderer _spriteRenderer;

        private void Start()
        {
            QteEnemyComponent = GetComponent<QteEnemy>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            // 颜色伪装
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originColor = _spriteRenderer.color;
            _originColor.a = 1f;
            _hideColor = _originColor;
            _hideColor.a = 0.1f;
            // 伪装状态初始化
            if (hideType == HideType.NoHide) return;
            _spriteRenderer.color = _hideColor;
        }

        /// <summary>
        /// 短暂解除隐身伪装
        /// </summary>
        private void RemoveTemporaryHide()
        {
            StartCoroutine(TemporaryHide());
        }

        private IEnumerator TemporaryHide()
        {
            _spriteRenderer.color = _originColor;
            yield return new WaitForSeconds(showTime);
            _spriteRenderer.color = _hideColor;
        }

        /// <summary>
        /// 获取伪装类型
        /// </summary>
        /// <returns></returns>
        public HideType GetHideType()
        {
            return hideType;
        }

        /// <summary>
        /// 解除当前怪物伪装
        /// </summary>
        public void RemoveHide()
        {
            switch (hideType)
            {
                case HideType.NoHide:
                    break;
                case HideType.Once:
                    _spriteRenderer.color = _originColor;
                    break;
                case HideType.Continuous:
                    RemoveTemporaryHide();
                    break;
                default:
                    Debug.LogWarning("HideType 伪装类型错误");
                    break;
            }
        }

        /// <summary>
        /// 解除整个链表的怪物伪装
        /// </summary>
        public void RemoveListEnemiesHide()
        {
            var objs = FindObjectsByType<QteEnemyHide>(FindObjectsSortMode.None);
            foreach (var obj in objs)
            {
                if (obj.QteEnemyComponent.linkedListId != QteEnemyComponent.linkedListId) continue;
                obj.RemoveHide();
            }
        }
    }
}
