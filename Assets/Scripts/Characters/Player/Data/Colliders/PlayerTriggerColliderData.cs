using System;
using UnityEngine;

namespace GenshinLike
{
    [Serializable]
    public class PlayerTriggerColliderData
    {
        [field: SerializeField] public BoxCollider GroundCheckCollider { get; private set; }
    }
}
