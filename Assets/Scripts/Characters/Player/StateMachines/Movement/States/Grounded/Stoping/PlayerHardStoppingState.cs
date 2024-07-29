using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinLike
{
    public class PlayerHardStoppingState : PlayerStoppingState
    {
        public PlayerHardStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        #region IState Methods
        public override void Enter()
        {
            base.Enter();

            stateMachine.ReusableData.MovementDecelerationForce = movemnetData.StopData.HardDecelerationForce;
        }
        #endregion

        #region Reusable Methods
        protected override void OnMove() // �ȱ� ����� Ȱ��ȭ �� ���, ��� ���·θ� ��ȯ ����
        {
            if (stateMachine.ReusableData.ShouldWalk)
            {
                return;
            }

            stateMachine.ChangeState(stateMachine.RunningState);
        }
        #endregion
    }
}
