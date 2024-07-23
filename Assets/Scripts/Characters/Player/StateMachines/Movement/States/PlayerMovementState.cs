using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinLike
{
    public class PlayerMovementState : IState
    {
        protected PlayerMovementStateMachine stateMachine;

        protected PlayerGroundedData movemnetData;

        public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
        {
            stateMachine = playerMovementStateMachine;

            movemnetData = stateMachine.Player.Data.GroundedData;

            InitializeData();
        }

        private void InitializeData()
        {
            stateMachine.ReusableData.TimeToReachTargetRotation = movemnetData.BaseRotationData.TargetRotationReachTime;
        }

        #region IState Methods(스테이트에 종속적인 메소드)
        public virtual void Enter()
        {
            Debug.Log("Enter: " + GetType().Name);

            AddInputActionsCallbacks();
        }

        public virtual void Exit()
        {
            Debug.Log("Exit: " + GetType().Name);

            RemoveInputActionsCallbacks();
        }

        public virtual void HandleInput()
        {
            ReadMovementInput();
        }

        public virtual void Update()
        {
            //Debug.Log(stateMachine.ReusableData.ShouldWalk.ToString());
        }

        public virtual void FixedUpdate()
        {
            Move();
        }
        #endregion

        #region Main Methods(실제 실행되는 메소드)
        private void ReadMovementInput()
        {
            stateMachine.ReusableData.MovementInput = stateMachine.Player.Input.PlayerActions.Movement.ReadValue<Vector2>();
        }


        private void Move()
        {
            if (stateMachine.ReusableData.MovementInput == Vector2.zero || stateMachine.ReusableData.MovementSpeedModifier == 0f)
                return;

            Vector3 movementDir = GetMovementInputDirection();

            // 플레이어가 움직이는 도중에만 회전
            float targetRotationYAngle = Rotate(movementDir);
            Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle); // 실제 회전 방향

            float movementSpeed = GetMovementSpeed();

            Vector3 curPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
            // 현재 속도에 힘을 추가하는 방식이므로 속도 유지를 위해 이전 속도를 뺀다
            stateMachine.Player.Rigidbody.AddForce(targetRotationDirection * movementSpeed - curPlayerHorizontalVelocity, ForceMode.VelocityChange);
        }

        private float Rotate(Vector3 direction)
        {
            float directionAngle = UpdateTargetRotation(direction);

            return directionAngle;
        }

        private static float GetDirectionAngle(Vector3 direction)
        {
            float directionAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            if (directionAngle < 0f)
                directionAngle += 360f;
            return directionAngle;
        }

        private float AddCameraRotationToAngle(float angle)
        {
            angle += stateMachine.Player.MainCameraTransform.eulerAngles.y;

            if (angle > 360f)
                angle -= 360f;

            return angle;
        }

        private void UpdateTargetRotationData(float targetAngle)
        {
            stateMachine.ReusableData.CurrentTargetRotation.y = targetAngle;

            stateMachine.ReusableData.DampedTargetRotationPassedTime.y = 0f;
        }
        #endregion

        #region Reusable Methods(재사용 가능 메소드)
        protected Vector3 GetMovementInputDirection()
        {
            return new Vector3(stateMachine.ReusableData.MovementInput.x, 0f, stateMachine.ReusableData.MovementInput.y);
        }

        protected float GetMovementSpeed()
        {
            return movemnetData.BaseSpeed * stateMachine.ReusableData.MovementSpeedModifier;
        }

        protected Vector3 GetPlayerHorizontalVelocity()
        {
            Vector3 playerHorizontalVelocity = stateMachine.Player.Rigidbody.velocity;
            playerHorizontalVelocity.y = 0;

            return playerHorizontalVelocity;
        }
         
        protected void RotateTowardsTargetRotation()
        {
            float currentYAngle = stateMachine.Player.Rigidbody.rotation.eulerAngles.y;

            // 현재 회전 방향이 목표 방향과 동일하다면 종료
            if (currentYAngle == stateMachine.ReusableData.CurrentTargetRotation.y)
                return;

            float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, stateMachine.ReusableData.CurrentTargetRotation.y,
                                                         ref stateMachine.ReusableData.DampedTargetRotationCurrentVelocity.y,
                                                         stateMachine.ReusableData.TimeToReachTargetRotation.y - stateMachine.ReusableData.DampedTargetRotationPassedTime.y);

            stateMachine.ReusableData.DampedTargetRotationPassedTime.y += Time.deltaTime; // fixedUpdate에서 사용되면 자동으로 Time.fixedDeltaTime으로 변경됨

            Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

            stateMachine.Player.Rigidbody.MoveRotation(targetRotation);
        }

        /// <summary>
        /// 입력받은 이동 방향을 통해 실제 회전 방향을 반환하는 메소드
        /// </summary>
        /// <param name="direction">키보드를 통해 입력된 방향</param>
        /// <param name="shouldConsiderCameraRotation">카메라 회전을 고려할 것인지에 대한 여부, 기본적으로 참</param>
        /// <returns></returns>
        protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
        {
            float directionAngle = GetDirectionAngle(direction);

            if(shouldConsiderCameraRotation)
                directionAngle = AddCameraRotationToAngle(directionAngle);

            // 만약 새로 입력받은 방향이 현재 목표 방향과 다를 경우 갱신 (즉, 목표 방향이 바뀌는 경우)
            if (directionAngle != stateMachine.ReusableData.CurrentTargetRotation.y)
            {
                UpdateTargetRotationData(directionAngle);
            }

            RotateTowardsTargetRotation();
            return directionAngle;
        }

        protected Vector3 GetTargetRotationDirection(float targetAngle)
        {
            return Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        protected void ResetVelocity()
        {
            stateMachine.Player.Rigidbody.velocity = Vector3.zero;
        }

        protected virtual void AddInputActionsCallbacks()
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started += OnWalkToggleStarted;
        }

        protected virtual void RemoveInputActionsCallbacks()
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started -= OnWalkToggleStarted;
        }

        #endregion

        #region Input Methods(입력 관련 메소드)
        protected virtual void OnWalkToggleStarted(InputAction.CallbackContext context)
        {
            stateMachine.ReusableData.ShouldWalk = !stateMachine.ReusableData.ShouldWalk;
        }

        #endregion
    }
}
