using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinLike
{
    public class PlayerGroundedState : PlayerMovementState
    {
        private SlopeData slopeData;

        public PlayerGroundedState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
        {
            slopeData = stateMachine.Player.ColliderUtility.SlopeData;
        }

        #region IState Methods
        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            Float();
        }
        #endregion

        #region Main Methods
        private void Float()
        {
            Vector3 capsuleColliderCenterInWorldSpace = stateMachine.Player.ColliderUtility.CapsuleColliderData.Collider.bounds.center;

            Ray downwardRayFromCapsuleCenter = new Ray(capsuleColliderCenterInWorldSpace, Vector3.down);

            if (Physics.Raycast(downwardRayFromCapsuleCenter, out RaycastHit hit, slopeData.FloatRayDistance,
                stateMachine.Player.LayerData.GroundLayer, QueryTriggerInteraction.Ignore)) // Environment 레이어에서 Trigger 속성은 제외
            {
                float groundAngle = Vector3.Angle(hit.normal, -downwardRayFromCapsuleCenter.direction);

                float slopeSpeedModifier = SetSlopeSpeedModifierOnAngle(groundAngle); // 경사에 따른 속도 적용

                if (slopeSpeedModifier == 0f) // 경사가 높아 속도가 0이라면 이동 불가능하게 변경
                {
                    return;
                }

                float distanceToFloatingPoint = stateMachine.Player.ColliderUtility.CapsuleColliderData.ColliderCenterInLocalSpace.y * stateMachine.Player.transform.localScale.y - hit.distance;

                if(distanceToFloatingPoint == 0f)
                {
                    return;
                }

                float amountToLift = distanceToFloatingPoint * slopeData.StepReachForce - GetPlayerVerticalVelocity().y;

                Vector3 liftForce = new Vector3(0f, amountToLift, 0f);

                stateMachine.Player.Rigidbody.AddForce(liftForce, ForceMode.VelocityChange);
            }

        }

        private float SetSlopeSpeedModifierOnAngle(float angle)
        {
            float slopeSpeedModifier = movemnetData.SlopeSpeedAngle.Evaluate(angle);

            stateMachine.ReusableData.MovementOnSlopeSpeedModifier = slopeSpeedModifier;

            return slopeSpeedModifier;
        }
        #endregion

        #region Reusable Methods
        protected override void AddInputActionsCallbacks() // 특정 상태에 들어갈 때, 입력에 대한 발생하는 메소드 추가
        {
            base.AddInputActionsCallbacks();

            stateMachine.Player.Input.PlayerActions.Movement.canceled += OnMovementCanceled;

            stateMachine.Player.Input.PlayerActions.Dash.started += OnDashStarted;
        }

        protected override void RemoveInputActionsCallbacks() // 특정 상태에서 나올 때, 입력에 대한 발생하는 메소드 삭제
        {
            base.RemoveInputActionsCallbacks();

            stateMachine.Player.Input.PlayerActions.Movement.canceled -= OnMovementCanceled;

            stateMachine.Player.Input.PlayerActions.Dash.started -= OnDashStarted;
        }

        protected virtual void OnMove()
        {
            // 걷기 토글이 켜져있다면 걷기 상태로, 아니면 달리기 상태로 진입
            if (stateMachine.ReusableData.ShouldWalk)
            {
                stateMachine.ChangeState(stateMachine.WalkingState);

                return;
            }

            stateMachine.ChangeState(stateMachine.RunningState);
        }
        #endregion

        #region Input Methods
        protected virtual void OnMovementCanceled(InputAction.CallbackContext context) // 이동 입력 종료 시, 대기 상태로 전환
        {
            stateMachine.ChangeState(stateMachine.IdlingState);
        }

        protected virtual void OnDashStarted(InputAction.CallbackContext context)
        {
            stateMachine.ChangeState(stateMachine.DashingState);
        }

        #endregion
    }
}
