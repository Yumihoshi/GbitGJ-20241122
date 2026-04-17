using System.Timers;
using GCFramework.Runtime.StateMachineCore;
using UnityEngine;

namespace GCat_Test.Core.EnemyStates
{
    public class EnemyIdleState : EnemyStateBase
    {
        private float _idleTimer;

        public override void Enter(StateContext stateContext)
        {
            _idleTimer = Random.Range(enemyCtr.dataModel.idleTime.x, enemyCtr.dataModel.idleTime.y);
            enemyCtr.EnemyMovement.SetRbVelocity(Vector2.zero);
        }

        public override void Update(StateContext stateContext)
        {
            base.Update(stateContext);
            if (enemyCtr == null)
                return;

            if (enemyCtr.GetPlayerTarget())
            {
                stateMachine.SwitchState<EnemyChaseState>();
                return;
            }

            _idleTimer -= Time.deltaTime;
            if (_idleTimer <= 0)
            {
                stateMachine.SwitchState<EnemyPatrolState>();
            }
        }
    }
}