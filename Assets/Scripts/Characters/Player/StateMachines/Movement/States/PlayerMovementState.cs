using System;
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
        protected Vector3 GetMovementInputDirection() // �Է��� �̵� ������ ������
        {
            return new Vector3(stateMachine.ReusableData.MovementInput.x, 0f, stateMachine.ReusableData.MovementInput.y);
        }

        protected float GetMovementSpeed() // �÷��̾��� ���� �ӷ��� ������ (�ӷ� ����� ���� �ӷ� ����� ���� ������)
        {
            return movemnetData.BaseSpeed * stateMachine.ReusableData.MovementSpeedModifier * stateMachine.ReusableData.MovementOnSlopeSpeedModifier;
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
        }

        protected virtual void RemoveInputActionsCallbacks() // �߰��� �ݹ� �޼ҵ带 ����
        {
            stateMachine.Player.Input.PlayerActions.WalkToggle.started -= OnWalkToggleStarted;
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

        protected bool isMovingUp(float minimumVelocity = 0.1f)
        {
            return GetPlayerVerticalVelocity().y > minimumVelocity;
        }

        protected bool isMovingDown(float minimumVelocity = 0.1f)
        {
            return GetPlayerVerticalVelocity().y < -minimumVelocity;
        }

        protected void SetBaseRotationData()
        {
            stateMachine.ReusableData.RotationData = movemnetData.BaseRotationData;

            stateMachine.ReusableData.TimeToReachTargetRotation = stateMachine.ReusableData.RotationData.TargetRotationReachTime;
        }

        protected virtual void OnContactWithGround(Collider collider)
        {
            
        }

        protected virtual void OnContactWithGroundExited(Collider collider)
        {
            
        }
        #endregion

        #region Input Methods(�Է� ���� �޼ҵ�)
        protected virtual void OnWalkToggleStarted(InputAction.CallbackContext context)
        {
            stateMachine.ReusableData.ShouldWalk = !stateMachine.ReusableData.ShouldWalk;
        }
        #endregion
    }
}
