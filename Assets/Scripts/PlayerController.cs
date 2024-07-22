using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 700f;
    public Transform cameraTransform;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // ī�޶��� ������ �������� �̵� ���� ����
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = forward * direction.z + right * direction.x;

            // �÷��̾� ȸ��
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotateSpeed, 0.1f);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            // �÷��̾� �̵�
            rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime);
        }
    }
}
