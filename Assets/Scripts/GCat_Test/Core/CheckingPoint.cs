using System;
using GCat_Test.Core.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GCat_Test.Core
{
    /// <summary>
    /// 检查点
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class CheckingPoint : MonoBehaviour
    {
        public int id;
        public bool isTrigger;
        [LabelText("复活点")] 
        public Transform revivalPoint;
        public Sprite activatedIcon;
        public Sprite notActivatedIcon;
        public UnityEvent<GameObject> onCheckingPointActivated;

        private Animator _animator;
        private SpriteRenderer _iconSpriteRenderer;
        private Collider2D _triggerCollider;
        [ShowInInspector, ReadOnly] private bool _isActivated;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _iconSpriteRenderer = transform.Find("icon").GetComponent<SpriteRenderer>();
            _triggerCollider = GetComponent<Collider2D>();
            revivalPoint = transform.Find("revival_point");
        }

        private void Start()
        {
            _triggerCollider.isTrigger = isTrigger;
            _triggerCollider.enabled = isTrigger;
            _iconSpriteRenderer.sprite = notActivatedIcon;
            _isActivated = false;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            bool key = Keyboard.current.eKey.wasPressedThisFrame;
            if (other.CompareTag("Player"))
            {
                if (!_isActivated && key)
                {
                    ActivateCheckingPoint(other.gameObject);
                }
                else if (key)
                {
                    Debug.Log("打开地图");
                }
            }
        }

        public void ActivateCheckingPoint(GameObject player)
        {
            _iconSpriteRenderer.sprite = activatedIcon;
            _isActivated = true;
            onCheckingPointActivated?.Invoke(player);
            GameLevelManager.Ins.AddCheckingPoint(this);
        }
    }
}