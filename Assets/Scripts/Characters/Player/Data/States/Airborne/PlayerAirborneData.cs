using System;
using UnityEngine;

namespace GenshinLike
{
    [Serializable]
    public class PlayerAirborneData
    {
        [field: SerializeField] public PlayerJumpData JumpData { get; private set; }
    }
}
