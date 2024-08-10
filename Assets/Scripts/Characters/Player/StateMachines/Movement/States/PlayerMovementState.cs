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

        #region IState Methods(������Ʈ�� �������� �޼ҵ�)
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

        #region Main Methods(���� ����Ǵ� �޼ҵ�)
        private void ReadMovementInput()
        {
            stateMachine.ReusableData.MovementInput = stateMachine.Player.Input.PlayerActions.Movement.ReadValue<Vector2>();
        }


        private void Move()
        {
            if (stateMachine.ReusableData.MovementInput == Vector2.zero || stateMachine.ReusableData.MovementSpeedModifier == 0f)
                return;

            Vector3 movementDir = GetMovementInputDirection();

            // �÷��̾ �����̴� ���߿��� ȸ��
            float targetRotationYAngle = Rotate(movementDir);
            Vector3 targetRotationDirection = GetTargetRotationDirection(targetRotationYAngle); // ���� ȸ�� ����

            float movementSpeed = GetMovementSpeed();

            Vector3 curPlayerHorizontalVelocity = GetPlayerHorizontalVelocity();
            // ���� �ӵ��� ���� �߰��ϴ� ����̹Ƿ� �ӵ� ������ ���� ���� �ӵ��� ����
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

        #region Reusable Methods(���� ���� �޼ҵ�)
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

        protected Vector3 GetMovementInputDirection() // �Է��� �̵� ������ ������
        {
            return new Vector3(stateMachine.ReusableData.MovementInput.x, 0f, stateMachine.ReusableData.MovementInput.y);
        }

        protected float GetMovementSpeed(bool shouldConsiderSlopes = true) // �÷��̾��� ���� �ӷ��� ������ (�ӷ� ����� ���� �ӷ� ����� ���� ������)
        {
            float movementSpeed = movemnetData.BaseSpeed * stateMachine.ReusableData.MovementSpeedModifier;

            if (shouldConsiderSlopes)
            {
                movementSpeed *= stateMachine.ReusableData.MovementOnSlopeSpeedModifier;
            }

            return movementSpeed;
        }

        protected Vector3 GetPlayerHorizontalVelocity() // �÷��̾��� ���� �ӵ��� ������
        {
            Vector3 playerHorizontalVelocity = stateMachine.Player.Rigidbody.velocity;
            playerHorizontalVelocity.y = 0;

            return playerHorizontalVelocity;
        }

        protected Vector3 GetPlayerVerticalVelocity() // �÷��̾��� ���� �ӵ��� ������
        {
            return new Vector3(0f, stateMachine.Player.Rigidbody.velocity.y, 0f);
        }
         
        protected void RotateTowardsTargetRotation()
        {
            float currentYAngle = stateMachine.Player.Rigidbody.rotation.eulerAngles.y;

            // ���� ȸ�� ������ ��ǥ ����� �����ϴٸ� ����
            if (currentYAngle == stateMachine.ReusableData.CurrentTargetRotation.y)
                return;

            float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, stateMachine.ReusableData.CurrentTargetRotation.y,
                                                         ref stateMachine.ReusableData.DampedTargetRotationCurrentVelocity.y,
                                                         stateMachine.ReusableData.TimeToReachTargetRotation.y - stateMachine.ReusableData.DampedTargetRotationPassedTime.y);

            stateMachine.ReusableData.DampedTargetRotationPassedTime.y += Time.deltaTime; // fixedUpdate���� ���Ǹ� �ڵ����� Time.fixedDeltaTime���� �����

            Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);

            stateMachine.Player.Rigidbody.MoveRotation(targetRotation);
        }

        /// <summary>
        /// �Է¹��� �̵� ������ ���� ���� ȸ�� ������ ��ȯ�ϴ� �޼ҵ�
        /// </summary>
        /// <param name="direction">Ű���带 ���� �Էµ� ����</param>
        /// <param name="shouldConsiderCameraRotation">ī�޶� ȸ���� ����� �������� ���� ����, �⺻������ ��</param>
        /// <returns></returns>
        protected float UpdateTargetRotation(Vector3 direction, bool shouldConsiderCameraRotation = true)
        {
            float directionAngle = GetDirectionAngle(direction);

            if(shouldConsiderCameraRotation)
                directionAngle = AddCameraRotationToAngle(directionAngle);

            // ���� ���� �Է¹��� ������ ���� ��ǥ ����� �ٸ� ��� ���� (��, ��ǥ ������ �ٲ�� ���)
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

        protected void ResetVelocity() // �÷��̾��� �ӵ��� 0���� �ʱ�ȭ
        {
            stateMachine.Player.Rigidbody.velocity = Vector3.zero;
        }

        protected void ResetVerticalVelocity() // �÷��̾��� ���� �ӵ��� 0���� �ʱ�ȭ
        {
            Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

            stateMachine.Player.Rigidbody.velocity = playerHorizontalVelocity;
        }

        protected virtual void AddInputActionsCallbacks() // �Է¿� ���� �ݹ� �޼ҵ带 �߰�
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started += OnWalkToggleStarted;

            stateMachine.Player.Input.PlayerActions.Look.started += OnMouseMovementStarted;

            stateMachine.Player.Input.PlayerActions.Movement.performed += OnMovementPerformed;
            stateMachine.Player.Input.PlayerActions.Movement.canceled += OnMovementCanceled;
        }

        protected virtual void RemoveInputActionsCallbacks() // �߰��� �ݹ� �޼ҵ带 ����
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started -= OnWalkToggleStarted;

            stateMachine.Player.Input.PlayerActions.Look.started -= OnMouseMovementStarted;

            stateMachine.Player.Input.PlayerActions.Movement.performed -= OnMovementPerformed;
            stateMachine.Player.Input.PlayerActions.Movement.canceled -= OnMovementCanceled;
        }

        protected void DecelerateHorizontally() // �÷��̾� ���� ���� �޼ҵ�
        {
            Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

            stateMachine.Player.Rigidbody.AddForce(-playerHorizontalVelocity * stateMachine.ReusableData.MovementDecelerationForce, ForceMode.Acceleration);
        }

        protected void DecelerateVertically() // �÷��̾� ���� ���� �޼ҵ�
        {
            Vector3 playerVerticalVelocity = GetPlayerVerticalVelocity();

            stateMachine.Player.Rigidbody.AddForce(-playerVerticalVelocity * stateMachine.ReusableData.MovementDecelerationForce, ForceMode.Acceleration);
        }

        protected bool IsMovingHorizontally(float minimumMagnitude = 0.1f) // �÷��̾��� ���� ������ ���� Ȯ�� �޼ҵ�
        {
            Vector3 playerHorizontalVelocity = GetPlayerHorizontalVelocity();

            Vector2 playerHorizontalMovement = new Vector2(playerHorizontalVelocity.x, playerHorizontalVelocity.z);

            return playerHorizontalMovement.magnitude > minimumMagnitude;
        }

        protected bool isMovingUp(float minimumVelocity = 0.1f) // �÷��̾��� ���� ��� ���� Ȯ�� �޼ҵ�
        {
            return GetPlayerVerticalVelocity().y > minimumVelocity;
        }

        protected bool isMovingDown(float minimumVelocity = 0.1f) // �÷��̾��� ���� �ϰ� ���� Ȯ�� �޼ҵ�
        {
            return GetPlayerVerticalVelocity().y < -minimumVelocity;
        }

        protected void SetBaseRotationData() // ȸ�� ������ ���� �޼ҵ�
        {
            stateMachine.ReusableData.RotationData = movemnetData.BaseRotationData;

            stateMachine.ReusableData.TimeToReachTargetRotation = stateMachine.ReusableData.RotationData.TargetRotationReachTime;
        }

        protected virtual void OnContactWithGround(Collider collider) // ����� �浹�ϴ� ��� �۵��ϴ� �޼ҵ�
        {
        }

        protected virtual void OnContactWithGroundExited(Collider collider) // ���鿡�� �������� ��� �۵��ϴ� �޼ҵ�
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

            // ���Ϸ� ���� �׻� ����� �����Ƿ� ���� ȸ������ ��� ���� �߰� �۾��� �� (-90 ~ 90)
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

        protected void EnableCameraRecentering(float waitTime = -1f, float recenteringTime = -1f) // ī�޶��� �߽� ���� ����� Ȱ��ȭ�ϴ� �޼ҵ�
        {
            float movementSpeed = GetMovementSpeed();

            if(movementSpeed == 0f)
            {
                movementSpeed = movemnetData.BaseSpeed;
            }

            stateMachine.Player.CameraUtility.EnableRecentering(waitTime, recenteringTime, movemnetData.BaseSpeed, movementSpeed);
        }

        protected void DisableCameraRecentering() // ī�޶��� �߽� ���� ����� ��Ȱ��ȭ�ϴ� �޼ҵ�
        {
            stateMachine.Player.CameraUtility.DisableRecentering();
        }

        // �ش� ������ ���� ī�޶� �߽� ������ Ȱ��ȭ �� ��Ȱ��ȭ�ϴ� �޼ҵ�
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

        #region Input Methods(�Է� ���� �޼ҵ�)
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
