using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenshinLike
{
    public class PlayerInput : MonoBehaviour
    {
        public PlayerInputAction InputActions { get; private set; }
        public PlayerInputAction.PlayerActions PlayerActions { get; private set; }

        private void Awake()
        {
            InputActions = new PlayerInputAction();

            PlayerActions = InputActions.Player;
        }

        private void OnEnable()
        {
            InputActions.Enable();
        }

        private void OnDisable()
        {
            InputActions.Disable();
        }

        public void DisableActionFor(InputAction action, float time)
        {
            StartCoroutine(DisableAction(action, time));
        }

        private IEnumerator DisableAction(InputAction action, float time)
        {
            action.Disable();

            yield return new WaitForSeconds(time);

            action.Enable();
        }
    }
}
