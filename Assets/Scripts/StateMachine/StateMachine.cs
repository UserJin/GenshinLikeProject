using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinLike
{
    public abstract class StateMachine
    {
        /// <summary>
        /// 현재 상태
        /// </summary>
        protected IState currentState;

        /// <summary>
        /// 다음 상태로 변화 시키는 메소드
        /// </summary>
        /// <param name="newState">전환하고자 하는 다음 상태</param>
        public void ChangeState(IState newState)
        {
            // 초기에는 currentState가 null이므로 이에 대한 예외 처리
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        public void HandleInput()
        {
            currentState?.HandleInput();
        }

        public void Update()
        {
            currentState?.Update();
        }

        public void FixedUpdate()
        {
            currentState?.FixedUpdate();
        }
    }

    
}
