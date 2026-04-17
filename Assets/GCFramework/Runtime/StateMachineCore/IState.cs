namespace GCFramework.Runtime.StateMachineCore
{
    public interface IState
    {
        public void Init(StateMachine stateMachine);
        public void Release();
        
        public void Enter(StateContext stateContext);
        public void Exit(StateContext stateContext);
        public void LateUpdate(StateContext stateContext);
        public void FixedUpdate(StateContext stateContext);
        public void Update(StateContext stateContext);
    }
}
