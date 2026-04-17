using GCFramework.Runtime.StateMachineCore;
using UnityEngine;

namespace GCat_Test.Core.EnemyStates
{
    public class EnemyPatrolState : EnemyStateBase
    {
        private Vector2 _nextTargetPos;
        private int _currentPointIdx;
        private float _distFromTarget;

        public override void Enter(StateContext stateContext)
        {
            base.Enter(stateContext);
            _nextTargetPos = enemyCtr.patrolPositions[_currentPointIdx];
        }

        public override void Update(StateContext stateContext)
        {
            base.Update(stateContext);

            if (enemyCtr.GetPlayerTarget())
            {
                stateMachine.SwitchState<EnemyChaseState>();
                return;
            }
            
            PatrolHandle();
            if (_distFromTarget < 0.5f)
            {
                _currentPointIdx++;
                _currentPointIdx %= enemyCtr.patrolPositions.Length;
                stateMachine.SwitchState<EnemyIdleState>();
            }
        }

        protected virtual void PatrolHandle()
        {
            float horizontal = (_nextTargetPos.x - ((Vector2)baseObject.position).x);
            _distFromTarget = Mathf.Abs(horizontal);
            horizontal = horizontal > 0 ? 1 : -1;

            enemyCtr.EnemyMovement.SetRbVelocity(Vector2.right * horizontal, enemyCtr.dataModel.moveSpeed);
        }
    }
}