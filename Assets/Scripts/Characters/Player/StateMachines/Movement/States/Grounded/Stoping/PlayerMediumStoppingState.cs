using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinLike
{
    public class PlayerMediumStoppingState : PlayerStoppingState
    {
        public PlayerMediumStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        #region IState Methods
        public override void Enter()
        {
            base.Enter();

            stateMachine.ReusableData.CurrentJumpForce = airborneData.JumpData.MediumForce;
            stateMachine.ReusableData.MovementDecelerationForce = movemnetData.StopData.MediumDecelerationForce;
        }
        #endregion
    }
}
