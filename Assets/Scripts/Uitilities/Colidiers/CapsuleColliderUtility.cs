using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinLike
{
    [Serializable]
    public class CapsuleColliderUtility
    {
        public CapsuleColliderData CapsuleColliderData { get; private set; }
        [field: SerializeField] public DefaultColliderData DefaultColliderData { get; private set; }
        [field: SerializeField] public SlopeData SlopeData { get; private set; }

        public void Initialize(GameObject gameObject)
        {
            if(CapsuleColliderData != null)
            {
                return;
            }

            CapsuleColliderData = new CapsuleColliderData();

            CapsuleColliderData.Initialize(gameObject);
        }

        public void CalculateCapsuleColliderDimensions()
        {
            SetCapsuleColliderRadius(DefaultColliderData.Radius);
            SetCapsuleColliderHeight(DefaultColliderData.Height * (1f - SlopeData.StepHeightPercentage));

            ReCalculateCapsuleColliderCenter();

            // �ݶ��̴� ������ ������ ���������� �۾��� ��� �ݶ��̴��� �̵��ϴ� ������ �ذ��ϱ� ���� �������� �缳��
            float halfColliderHeight = CapsuleColliderData.Collider.height / 2f;
            if (halfColliderHeight < CapsuleColliderData.Collider.radius)
            {
                CapsuleColliderData.Collider.radius = halfColliderHeight;
            }

            CapsuleColliderData.UpdateColliderData();
        }

        private void SetCapsuleColliderRadius(float radius)
        {
            CapsuleColliderData.Collider.radius = radius;
        }

        private void SetCapsuleColliderHeight(float height)
        {
            CapsuleColliderData.Collider.height = height;
        }

        private void ReCalculateCapsuleColliderCenter()
        {
            float colliderHeightDifference = DefaultColliderData.Height - CapsuleColliderData.Collider.height;

            Vector3 newColliderCenter = new Vector3(0f, DefaultColliderData.CenterY + (colliderHeightDifference / 2f), 0f);

            CapsuleColliderData.Collider.center = newColliderCenter;
        }

    }
}
