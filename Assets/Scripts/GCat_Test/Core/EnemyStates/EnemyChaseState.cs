using GCat_Test.Core.Damage;
using GCFramework.Runtime.StateMachineCore;
using UnityEngine;

namespace GCat_Test.Core.EnemyStates
{
    public class EnemyChaseState : EnemyStateBase
    {
        private Transform _target;
        private float _attackIntervalTimer;
        private float _rotTimer;
        private bool _isAttacked;
        

        public override void Init(StateMachine stateMachine)
        {
            base.Init(stateMachine);
            enemyCtr.GetComponent<DamageObject>().onDamageHit.AddListener(OnDamageHit);
        }

        public override void Enter(StateContext stateContext)
        {
            base.Enter(stateContext);
        }

        public override void Update(StateContext stateContext)
        {
            base.Update(stateContext);
            if (enemyCtr == null)
                return;
            _target = enemyCtr.GetPlayerTarget()?.transform;

            if (_target == null)
            {
                stateMachine.SwitchState<EnemyIdleState>();
                return;
            }
            
            if (_attackIntervalTimer > 0)
            {
                _attackIntervalTimer -= Time.deltaTime;
                if (_attackIntervalTimer <= 0)
                {
                    _isAttacked = false;
                }
            }
            
            if (!_isAttacked) ChaseHandle();
        }

        private void ChaseHandle()
        {
            Vector2 dir = _target.position - baseObject.position;
            dir.y = 0;
            enemyCtr.EnemyMovement.SetRbVelocity(dir, enemyCtr.dataModel.moveSpeed);
        }
        
        private void OnDamageHit(Collider2D target)
        {
            _isAttacked = true;
            _attackIntervalTimer = enemyCtr.dataModel.attackInterval;
        }
    }
}