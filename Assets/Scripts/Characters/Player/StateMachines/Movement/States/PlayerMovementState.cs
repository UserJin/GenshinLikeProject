using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinLike
{
    public class PlayerMovementState : IState
    {
        protected PlayerMovementStateMachine stateMachine;

        protected PlayerGroundedData movemnetData;
        protected PlayerAirborneData airborneData;

        public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
        {
            stateMachine = playerMovementStateMachine;

            movemnetData = stateMachine.Player.Data.GroundedData;
            airborneData = stateMachine.Player.Data.AirborneData;
            SetBaseCameraRecenteringData();

            InitializeData();
        }

        private void InitializeData()
        {
            SetBaseRotationData();
        }

        #region IState Methods(스테이트에 종속적인 메소드)
        public virtual void Enter()
        {
            Debug.Log("Enter: " + GetType().Name);

            AddInputActionsCallbacks();
        }

        public virtual void Exit()
        {
            //Debug.Log("Exit: " + GetType().Name);

            RemoveInputActionsCallbacks();
        }

        public virtual void HandleInput()
        {
            ReadMovementInput();
        }

        public virtual void Update()
        {
        }

        public virtual void PhysicsUpdate()
        {
            Move();
        }

        public virtual void OnAnimationEnterEvent()
        {
        }

        public virtual void OnAnimationExitEvent()
        {
        }

        public virtual void OnAnimationTransitionEvent()
        {
        }

        public virtual void OnTriggerEnter(Collider collider)
        {
            if(stateMachine.Player.LayerData.IsGroundLayer(collider.gameObject.layer))
            {
                OnContactWithGround(collider);

                return;
            }
        }

        public virtual void OnTriggerExit(Collider collider)
        {
            if (stateMachine.Player.LayerData.IsGroundLayer(collider.gameObject.layer))
            {
                OnContactWithGroundExited(collider);

                return;
            }
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
        protected void StartAnimation(int animationHash)
        {
            stateMachine.Player.Animator.SetBool(animationHash, true);
        }

        protected void StopAnimation(int animationHash)
        {
            stateMachine.Player.Animator.SetBool(animationHash, false);
        }

        protected void SetBaseCameraRecenteringData()
        {
            stateMachine.ReusableData.BackwardsCameraRecenteringData = movemnetData.BackwardsCameraRecenteringData;
            stateMachine.ReusableData.SidewaysCameraRecenteringData = movemnetData.SidewaysCameraRecenteringData;
        }

        protected Vector3 GetMovementInputDirection() // 입력한 이동 방향을 가져옴
        {
            return new Vector3(stateMachine.ReusableData.MovementInput.x, 0f, stateMachine.ReusableData.MovementInput.y);
        }

        protected float GetMovementSpeed(bool shouldConsiderSlopes = true) // 플레이어의 현재 속력을 가져옴 (속력 배수와 경사면 속력 배수에 의해 결정됨)
        {
            float movementSpeed = movemnetData.BaseSpeed * stateMachine.ReusableData.MovementSpeedModifier;

            if (shouldConsiderSlopes)
            {
                movementSpeed *= stateMachine.ReusableData.MovementOnSlopeSpeedModifier;
            }

            return movementSpeed;
        }

        protected Vector3 GetPlayerHorizontalVelocity() // 플레이어의 수평 속도를 가져옴
        {
            Vector3 playerHorizontalVelocity = stateMachine.Player.Rigidbody.velocity;
            playerHorizontalVelocity.y = 0;

            return playerHorizontalVelocity;
        }

        protected Vector3 GetPlayerVerticalVelocity() // 플레이어의 수직 속도를 가져옴
        {
            return new Vector3(0f, stateMachine.Player.Rigidbody.velocity.y, 0f);
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

        protected void ResetVelocity() // 플레이어의 속도를 0으로 초기화
        {
            stateMachine.Player.Rigidbody.velocity = Vector3.zero;
        }

        protected void ResetVerticalVelocity() // 플레이어의 수직 속도를 0으로 초기화
        {
            Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

            stateMachine.Player.Rigidbody.velocity = playerHorizontalVelocity;
        }

        protected virtual void AddInputActionsCallbacks() // 입력에 의한 콜백 메소드를 추가
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started += OnWalkToggleStarted;

            stateMachine.Player.Input.PlayerActions.Look.started += OnMouseMovementStarted;

            stateMachine.Player.Input.PlayerActions.Movement.performed += OnMovementPerformed;
            stateMachine.Player.Input.PlayerActions.Movement.canceled += OnMovementCanceled;
        }

        protected virtual void RemoveInputActionsCallbacks() // 추가한 콜백 메소드를 제거
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started -= OnWalkToggleStarted;

            stateMachine.Player.Input.PlayerActions.Look.started -= OnMouseMovementStarted;

            stateMachine.Player.Input.PlayerActions.Movement.performed -= OnMovementPerformed;
            stateMachine.Player.Input.PlayerActions.Movement.canceled -= OnMovementCanceled;
        }

        protected void DecelerateHorizontally() // 플레이어 수평 감속 메소드
        {
            Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

            stateMachine.Player.Rigidbody.AddForce(-playerHorizontalVelocity * stateMachine.ReusableData.MovementDecelerationForce, ForceMode.Acceleration);
        }

        protected void DecelerateVertically() // 플레이어 수직 감속 메소드
        {
            Vector3 playerVerticalVelocity = GetPlayerVerticalVelocity();

            stateMachine.Player.Rigidbody.AddForce(-playerVerticalVelocity * stateMachine.ReusableData.MovementDecelerationForce, ForceMode.Acceleration);
        }

        protected bool IsMovingHorizontally(float minimumMagnitude = 0.1f) // 플레이어의 수평 움직임 여부 확인 메소드
        {
            Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

            Vector2 playerHorizontalMovement = new Vector2(playerHorizontalVelocity.x, playerHorizontalVelocity.z);

            return playerHorizontalMovement.magnitude > minimumMagnitude;
        }

        protected bool isMovingUp(float minimumVelocity = 0.1f) // 플레이어의 수직 상승 여부 확인 메소드
        {
            return GetPlayerVerticalVelocity().y > minimumVelocity;
        }

        protected bool isMovingDown(float minimumVelocity = 0.1f) // 플레이어의 수직 하강 여부 확인 메소드
        {
            return GetPlayerVerticalVelocity().y < -minimumVelocity;
        }

        protected void SetBaseRotationData() // 회전 데이터 설정 메소드
        {
            stateMachine.ReusableData.RotationData = movemnetData.BaseRotationData;

            stateMachine.ReusableData.TimeToReachTargetRotation = stateMachine.ReusableData.RotationData.TargetRotationReachTime;
        }

        protected virtual void OnContactWithGround(Collider collider) // 지면과 충돌하는 경우 작동하는 메소드
        {
        }

        protected virtual void OnContactWithGroundExited(Collider collider) // 지면에서 떨어지는 경우 작동하는 메소드
        {
        }

        protected void UpdateCameraRecenteringState(Vector2 movementInput)
        {
            if(movementInput == Vector2.zero)
            {
                return;
            }

            if(movementInput == Vector2.up)
            {
                DisableCameraRecentering();

                return;
            }

            float cameraVerticalAngle = stateMachine.Player.MainCameraTransform.eulerAngles.x;

            // 오일러 각은 항상 양수로 나오므로 음수 회전값을 얻기 위해 추가 작업을 함 (-90 ~ 90)
            if(cameraVerticalAngle >= 270f)
            {
                cameraVerticalAngle -= 360f;
            }

            cameraVerticalAngle = Mathf.Abs(cameraVerticalAngle);

            if (movementInput == Vector2.down)
            {
                SetCameraRecenteringState(cameraVerticalAngle, stateMachine.ReusableData.BackwardsCameraRecenteringData);

                return;
            }

            SetCameraRecenteringState(cameraVerticalAngle, stateMachine.ReusableData.SidewaysCameraRecenteringData);

        }

        protected void EnableCameraRecentering(float waitTime = -1f, float recenteringTime = -1f) // 카메라의 중심 설정 기능을 활성화하는 메소드
        {
            float movementSpeed = GetMovementSpeed();

            if(movementSpeed == 0f)
            {
                movementSpeed = movemnetData.BaseSpeed;
            }

            stateMachine.Player.CameraUtility.EnableRecentering(waitTime, recenteringTime, movemnetData.BaseSpeed, movementSpeed);
        }

        protected void DisableCameraRecentering() // 카메라의 중심 설정 기능을 비활성화하는 메소드
        {
            stateMachine.Player.CameraUtility.DisableRecentering();
        }

        // 해당 각도를 통해 카메라 중심 설정을 활성화 및 비활성화하는 메소드
        protected void SetCameraRecenteringState(float cameraVerticalAngle, List<PlayerCameraRecenteringData> cameraRecenteringData)
        {
            foreach (PlayerCameraRecenteringData recenteringData in cameraRecenteringData)
            {
                if (!recenteringData.IsWithinRange(cameraVerticalAngle))
                {
                    continue;
                }

                EnableCameraRecentering(recenteringData.WaitTime, recenteringData.RecenteringTime);

                return;
            }

            DisableCameraRecentering();
        }

        #endregion

        #region Input Methods(입력 관련 메소드)
        protected virtual void OnWalkToggleStarted(InputAction.CallbackContext context)
        {
            stateMachine.ReusableData.ShouldWalk = !stateMachine.ReusableData.ShouldWalk;
        }

        protected virtual void OnMovementCanceled(InputAction.CallbackContext context)
        {
            DisableCameraRecentering();
        }

        private void OnMouseMovementStarted(InputAction.CallbackContext context)
        {
            UpdateCameraRecenteringState(stateMachine.ReusableData.MovementInput);
        }

        protected virtual void OnMovementPerformed(InputAction.CallbackContext context)
        {
            UpdateCameraRecenteringState(context.ReadValue<Vector2>());
        }

        #endregion
    }
}
