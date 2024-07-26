using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinLike
{
    // Player ������Ʈ�� �߰��Ϸ��� �ݵ�� PlayerInput�� �־�� ��(������ �ڵ����� �߰���)
    [RequireComponent(typeof(PlayerInput))]
    public class Player : MonoBehaviour
    {
        [field: Header("References")]
        [field: SerializeField] public PlayerSO Data { get; private set; }

        [field: Header("Collisions")]
        [field: SerializeField] public CapsuleColliderUtility ColliderUtility { get; private set; }
        [field: SerializeField] public PlayerLayerData LayerData { get; private set; }

        public Rigidbody Rigidbody { get; private set; }
        public Transform MainCameraTransform { get; private set; }
        public PlayerInput Input { get; private set; }

        private PlayerMovementStateMachine movementStateMachine;

        private void Awake()
        {
            Input = GetComponent<PlayerInput>();
            Rigidbody = GetComponent<Rigidbody>();

            ColliderUtility.Initialize(gameObject);
            ColliderUtility.CalculateCapsuleColliderDimensions();

            MainCameraTransform = Camera.main.transform;

            movementStateMachine = new PlayerMovementStateMachine(this);
        }

        private void OnValidate()
        {
            ColliderUtility.Initialize(gameObject);
            ColliderUtility.CalculateCapsuleColliderDimensions();
        }

        private void Start()
        {
            // ó�� ���¸� Idle�� ����
            movementStateMachine.ChangeState(movementStateMachine.IdlingState);
        }

        private void Update()
        {
            movementStateMachine.HandleInput();

            movementStateMachine.Update();
        }

        private void FixedUpdate()
        {
            movementStateMachine.PhysicsUpdate();
        }
    }
}
