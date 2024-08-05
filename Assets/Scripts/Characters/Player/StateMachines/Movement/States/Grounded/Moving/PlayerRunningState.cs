using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinLike
{
    public class PlayerRunningState : PlayerMovingState
    {
        private float startTime;

        public PlayerRunningState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
        }

        #region IState Methods
        public override void Enter()
        {
            base.Enter();

            stateMachine.ReusableData.MovementSpeedModifier = movemnetData.RunData.SpeedModifier;
            stateMachine.ReusableData.CurrentJumpForce = airborneData.JumpData.MediumForce;

            startTime = Time.time;
        }

        public override void Update()
        {
            base.Update();

            if(!stateMachine.ReusableData.ShouldWalk)
            {
                return;
            }

            if(Time.time < startTime + movemnetData.RunData.RunToWalkTime)
            {
                return;
            }

            StopRunning();
        }
        #endregion

        #region Main Methods
        private void StopRunning()
        {
            if(stateMachine.ReusableData.MovementInput == Vector2.zero)
            {
                stateMachine.ChangeState(stateMachine.IdlingState);
                Debug.Log("aaa");
                return;
            }

            stateMachine.ChangeState(stateMachine.WalkingState);
        }
        #endregion

        #region Input Methods
        protected override void OnMovementCanceled(InputAction.CallbackContext context)
        {
            stateMachine.ChangeState(stateMachine.MediumStoppingState);
        }

        protected override void OnWalkToggleStarted(InputAction.CallbackContext context)
        {
            base.OnWalkToggleStarted(context);

            stateMachine.ChangeState(stateMachine.WalkingState);
        }
        #endregion
    }
}
