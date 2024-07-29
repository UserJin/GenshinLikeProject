using System;
using UnityEngine;

namespace GenshinLike
{
    [Serializable]
    public class PlayerSprintData
    {
        [field: SerializeField] [field: Range(1f, 3f)] public float SpeedModifier { get; private set; } = 1.7f;
        [field: SerializeField] [field: Range(0f, 5f)] public float SprintToRunTime { get; private set; } = 1f;
    }
}
