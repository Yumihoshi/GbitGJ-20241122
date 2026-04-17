namespace GCFramework.Runtime.StateMachineCore
{
    public abstract class StateBase : IState
    {
        protected StateMachine stateMachine;
        
        public virtual void Init(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public virtual void Release()
        {
        }

        #region 生命周期函数

        public virtual void Enter(StateContext stateContext)
        {
        }

        public virtual void Exit(StateContext stateContext)
        {
        }

        public virtual void LateUpdate(StateContext stateContext)
        {
        }

        public virtual void FixedUpdate(StateContext stateContext)
        {
        }

        public virtual void Update(StateContext stateContext)
        {
        }

        #endregion
    }
}