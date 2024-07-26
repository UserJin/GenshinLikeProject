using System;
using UnityEngine;

namespace GenshinLike
{
    [Serializable]
    public class DefaultColliderData
    {
        [field: SerializeField] public float Height { get; private set; } = 1.83f;
        [field: SerializeField] public float CenterY { get; private set; } = 0.915f;
        [field: SerializeField] public float Radius { get; private set; } = 0.2f;
    }
}
