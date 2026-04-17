using System.Collections;
using GCat_Test.Core.Damage;
using GCat_Test.Core.DataModel;
using GCat_Test.Core.EnemyStates;
using GCFramework.Runtime.StateMachineCore;
using UnityEngine;

namespace GCat_Test.Core
{
    public class CommonEnemyCtr : MonoBehaviour, IStateMachineOwner, IDamage
    {
        public Vector2 detectRange = new Vector2(2, 2);
        public LayerMask detectLayer;
        public CommonEnemyDataModel dataModel;
        [SerializeField] private Transform[] patrolPoints;

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        
        public CommonEnemyMovement EnemyMovement { get; protected set; }
        public StateMachine StateMachine { get; protected set; }
        
        public bool IsDead { get; private set; }

        [HideInInspector] public Vector2[] patrolPositions;

        private void Awake()
        {
            EnemyMovement = GetComponent<CommonEnemyMovement>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            patrolPositions = new Vector2[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                patrolPositions[i] = patrolPoints[i].position;
            }

            IsDead = false;
            InitStateMachine();
        }

        private void InitStateMachine()
        {
            StateMachine = new StateMachine(this);

            StateMachine.AddState<EnemyIdleState>();
            StateMachine.AddState<EnemyPatrolState>();
            StateMachine.AddState<EnemyChaseState>();

            StateMachine.Init<EnemyIdleState>();
        }

        private void Update()
        {
            if (IsDead)
                return;
            StateMachine.Update();
        }

        private void FixedUpdate()
        {
            if (IsDead)
                return;
            
            StateMachine.FixedUpdate();
        }

        private void LateUpdate()
        {
            if (IsDead)
                return;
            StateMachine.LateUpdate();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (dataModel)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, new Vector3(detectRange.x, detectRange.y));
            }
        }
#endif

        #region 伤害接口

        /// <summary>
        /// 对敌人造成伤害
        /// </summary>
        public void TakeDamage()
        {
            IsDead = true;
            _animator.SetTrigger("isDead");
            StateMachine.Release();
        }

        #endregion

        public void DestroySelf()
        {
            StartCoroutine(DelayDestroy(2f));
        }

        private IEnumerator DelayDestroy(float fadeTime)
        {
            float alpha = _spriteRenderer.color.a;
            while (alpha > 0)
            {
                alpha -= fadeTime * Time.deltaTime;
                var color = _spriteRenderer.color;
                color.a = alpha;
                _spriteRenderer.color = color;
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
    
    public static class CommonEnemyCtrHelper
    {
        public static Collider2D GetPlayerTarget(this CommonEnemyCtr enemyCtr)
        {
            var collider2D = Physics2D.OverlapBox(enemyCtr.transform.position, enemyCtr.detectRange, 0, enemyCtr.detectLayer);
            if (collider2D == null || !collider2D.CompareTag("Player"))
                return null;
            return collider2D;
        }
    }
}