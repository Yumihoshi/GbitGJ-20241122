using GCFramework.Runtime.StateMachineCore;
using UnityEngine;

namespace GCat_Test.Core
{
    public class EnemyStateBase : StateBase
    {
        protected CommonEnemyCtr enemyCtr;
        protected Transform baseObject;

        public override void Init(StateMachine stateMachine)
        {
            base.Init(stateMachine);

            enemyCtr = (CommonEnemyCtr)stateMachine.Owner;
            baseObject = enemyCtr.transform;
        }
    }
}