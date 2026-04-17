// ********************************************************************************
// @author: Starry Sky
// @email: xiaoyuesun915@gmail.com
// @creationDate: 2024/11/24 11:11
// @version: 1.0
// @description:
// ********************************************************************************

using System;
using System.Collections;
using Aki_Test.Character_Stats;
using Aki_Test.Managers;
using Aki_Test.Skills;
using JetBrains.Annotations;
using Tsuki_Test.Common;
using UnityEngine;

namespace Tsuki_Test.Enemy
{
    public class QteEnemy : MonoBehaviour, IQteable
    {
        public QteEnemyHide QteEnemyHideComponent { get; private set; }
        [Header("链表")]
        public int linkedListId; // 链表 ID
        [CanBeNull] public QteEnemy prev; // 前一个节点
        [CanBeNull] public QteEnemy next; // 后一个节点
        // 当前链表节点
        public DoublyLinkedListNode<QteEnemy> CurrentNode { get; private set; }

        [CanBeNull] public event Action OnQteableCallback; // QTE 可触发时回调事件
        [CanBeNull] public event Action<QteEnemy> OnPlayerEnterScopeCallback; // 玩家进入外层Collider时触发

        // 外层碰撞器是否触发
        private bool IsOuterTriggered
        {
            get => _isOuterTriggered;
            set
            {
                _isOuterTriggered = value;
                if (_isQteable) OnQteableCallback?.Invoke();
            }
        }

        // 内层碰撞器是否触发
        private bool IsInnerTriggered
        {
            get => _isInnerTriggered;
            set
            {
                _isInnerTriggered = value;
                if (_isQteable) OnQteableCallback?.Invoke();
            }
        }

        private bool _isOuterTriggered;
        private bool _isInnerTriggered;
        private Collider2D _collider;

        private QteEnemyInnerCollider _innerCollider;

        // 是否可以触发 QTE
        private bool _isQteable => IsOuterTriggered && !IsInnerTriggered;

        private void Awake()
        {
            IsOuterTriggered = false;
            IsInnerTriggered = false;
            // 链表初始化
            CurrentNode = new DoublyLinkedListNode<QteEnemy>(this);
            // 伪装事件注册
            
            if (SkillManager.Instance.Skills.Find(skill => skill.GetType() == typeof(Eliminate)))
            {
                OnPlayerEnterScopeCallback += GameObject.FindWithTag("Player").GetComponent<Eliminate>().GetEnemy;
            }
        }

        private void Start()
        {
            QteEnemyHideComponent = GetComponent<QteEnemyHide>();
            GameObject innerCollider = transform.Find("InnerCollider").gameObject;
            _innerCollider = innerCollider.GetComponent<QteEnemyInnerCollider>();
            _innerCollider.OnTriggerEnter2DEvent += OnInnerTriggerEnter2D;
            _innerCollider.OnTriggerExit2DEvent += OnInnerTriggerExit2D;
            // 链表头尾处理
            if (prev) CurrentNode.InsertHead(prev.CurrentNode);
            if (next) CurrentNode.InsertTail(next.CurrentNode);
        }

        /// <summary>
        /// 外层碰撞器触发
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerEnterScopeCallback?.Invoke(this);
                IsOuterTriggered = true;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerEnterScopeCallback?.Invoke(this);
                IsOuterTriggered = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerEnterScopeCallback?.Invoke(null);
                IsOuterTriggered = false;
            }
        }

        /// <summary>
        /// 内层碰撞器触发
        /// </summary>
        /// <param name="other"></param>
        private void OnInnerTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) IsInnerTriggered = true;
            CauseDamage();
        }

        private void OnInnerTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) IsInnerTriggered = false;
        }

        private void OnDestroy()
        {
            _innerCollider.OnTriggerEnter2DEvent -= OnInnerTriggerEnter2D;
            _innerCollider.OnTriggerExit2DEvent -= OnInnerTriggerExit2D;
        }

        private void OnDisable()
        {
            _innerCollider.OnTriggerEnter2DEvent -= OnInnerTriggerEnter2D;
            _innerCollider.OnTriggerExit2DEvent -= OnInnerTriggerExit2D;
        }

        /// <summary>
        /// 获取反弹向量
        /// </summary>
        /// <param name="player">玩家transform</param>
        /// <returns></returns>
        public Vector2 GetBounceVector(Transform player)
        {
            if (next == null) return Vector2.zero;
            return next.gameObject.transform.position - player.position;
        }

        /// <summary>
        /// Qte失败，给玩家造成伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        public void CauseDamage(int damage = 5)
        {
            var player = GameObject.FindWithTag("Player");
            player.GetComponent<CharacterStats>().TakeDamage(damage);
        }

        /// <summary>
        /// 获取 QTE 可触发状态
        /// </summary>
        /// <returns></returns>
        public bool GetQteable()
        {
            return _isQteable;
        }

        /// <summary>
        /// 当前怪物死亡
        /// </summary>
        public void KillSelf()
        {
            if (prev != null) return;
            if (next != null)
            {
                next.prev = null;
            }
            CurrentNode.RemoveNode();
            Destroy(gameObject);
        }

        /// <summary>
        /// 整个链表怪物死亡
        /// </summary>
        public void KillListEnemies()
        {
            var objs = FindObjectsByType<QteEnemy>(FindObjectsSortMode.None);
            foreach (var obj in objs)
            {
                if (obj.linkedListId != linkedListId || obj.gameObject.name == gameObject.name) continue;
                obj.KillSelf();
            }

            KillSelf();
        }
    }
}
