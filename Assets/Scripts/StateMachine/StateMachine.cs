using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinLike
{
    public abstract class StateMachine
    {
        /// <summary>
        /// ���� ����
        /// </summary>
        protected IState currentState;

        /// <summary>
        /// ���� ���·� ��ȭ ��Ű�� �޼ҵ�
        /// </summary>
        /// <param name="newState">��ȯ�ϰ��� �ϴ� ���� ����</param>
        public void ChangeState(IState newState)
        {
            // �ʱ⿡�� currentState�� null�̹Ƿ� �̿� ���� ���� ó��
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

        public void PhysicsUpdate()
        {
            currentState?.PhysicsUpdate();
        }

        public void OnAnimationEnterEvent()
        {
            currentState?.OnAnimationEnterEvent();
        }

        public void OnAnimationExitEvent()
        {
            currentState?.OnAnimationExitEvent();
        }

        public void OnAnimationTransitionEvent()
        {
            currentState?.OnAnimationTransitionEvent();
        }

        public void OnTriggerEnter(Collider collider)
        {
            currentState?.OnTriggerEnter(collider);
        }
    }

    
}
