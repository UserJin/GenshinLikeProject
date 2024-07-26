using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinLike
{
    public class BoundChecker : MonoBehaviour
    {
        SkinnedMeshRenderer skinnedMeshRenderer;

        private void Awake()
        {
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        }

        void Start()
        {
            Debug.Log(skinnedMeshRenderer.bounds.size);
        }

        void Update()
        {
        
        }
    }
}
