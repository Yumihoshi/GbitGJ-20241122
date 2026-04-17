using System;
using System.Collections.Generic;
using UnityEngine;

namespace GCFramework.Runtime.StateMachineCore
{
    public interface IStateMachineOwner
    {
        public GameObject gameObject { get; }
        public StateMachine StateMachine { get; }
    }

    public class StateContext
    {
        public StateBase lastState;
        public IStateMachineOwner owner;
    }
    
    public class StateMachine
    {
        #region Variables

        /// <summary>
        /// 当前状态
        /// </summary>
        public StateBase CurrentState { get; protected set; }
        
        public StateContext StateContext { get; protected set; }
     
        /// <summary>
        /// 谁的状态机
        /// </summary>
        public IStateMachineOwner Owner { get; protected set; }

        /// <summary>
        /// 状态记录字典
        /// </summary>
        private Dictionary<Type, StateBase> _stateDic = new Dictionary<Type, StateBase>();
        
        public bool IsInitialized { get; protected set; }

        private Action<StateContext> _updateAction;
        private Action<StateContext> _fixedUpdateAction;
        private Action<StateContext> _lateUpdateAction;

        #endregion
        /// <summary>
        /// 构装状态机
        /// </summary>
        /// <param itemName="owner"></param>
        public StateMachine(IStateMachineOwner owner)
        {
            this.Owner = owner;
            StateContext = new StateContext
            {
                owner = Owner
            };
        }

        public bool IsCurState<T>() where T : StateBase
        {
            return CurrentState is T;
        }
        
        public bool HasState<T>() where T : StateBase
        {
            return _stateDic.ContainsKey(typeof(T));
        }

        public T GetState<T>() where T : StateBase
        {
            return _stateDic.TryGetValue(typeof(T), out StateBase state) ? (T)state : null;
        }

        #region Add & Remove

        public void AddState<T>() where T : StateBase, new()
        {
            if (!_stateDic.ContainsKey(typeof(T)))
            {
                StateBase state = new T();
                state.Init(this);
                _stateDic.Add(typeof(T), state);
            }
        }

        public void RemoveState<T>()
        {
            if (_stateDic.ContainsKey(typeof(T)))
            {
                _stateDic.Remove(typeof(T));
            }
        }

        #endregion

        #region Enter & Exit & Update

        public void EnterState<T>() where T : StateBase
        {
            Type type = typeof(T);
            if (_stateDic.TryGetValue(type, out var state))
            {
                CurrentState = state;
                state.Enter(StateContext);
                _updateAction += CurrentState.Update;
                _fixedUpdateAction += CurrentState.FixedUpdate;
                _lateUpdateAction += CurrentState.LateUpdate;
            }
        }

        public void ExitState()
        {
            _updateAction -= CurrentState.Update;
            _fixedUpdateAction -= CurrentState.Update;
            _lateUpdateAction -= CurrentState.LateUpdate;
            StateContext.lastState = null;
            CurrentState.Exit(StateContext);
        }

        #endregion

        #region 生命周期

        public virtual void Update()
        {
            if (!IsInitialized)
                return;
            
            _updateAction?.Invoke(StateContext);
        }

        public virtual void FixedUpdate()
        {
            if (!IsInitialized)
                return;
            
            _fixedUpdateAction?.Invoke(StateContext);
        }

        public virtual void LateUpdate()
        {
            if (!IsInitialized)
                return;
            
            _lateUpdateAction?.Invoke(StateContext);
        }

        #endregion

        /// <summary>
        /// 状态机必须初始化后才能运行
        /// </summary>
        /// <typeparam name="T">可设置初始化状态</typeparam>
        public void Init<T>() where T : StateBase
        {
            EnterState<T>();
            IsInitialized = true;
        }

        public void SwitchState<T>() where T: StateBase
        {
            if (_stateDic.ContainsKey(typeof(T)))
            {
                ExitState();
                
                StateContext.lastState = CurrentState;

                EnterState<T>();
                Debug.LogWarning($"{Owner.gameObject.name} 切换到 {typeof(T).Name} 状态");
            }
        }

        public void Release()
        {
            foreach (var state in _stateDic)
            {
                state.Value.Release();
            }

            ExitState();
            CurrentState = null;
            _stateDic.Clear();
            _stateDic = null;
            IsInitialized = false;
        }

    }
}