using Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private InputSystemActions inputActions;

        public float MoveInput { get; private set; }
        public float VerticalInput { get; private set; }
        public bool JumpPressed => inputActions.Player.Jump.triggered;
        public bool JumpHeld => inputActions.Player.Jump.IsPressed();
        public bool JumpReleased => inputActions.Player.Jump.WasReleasedThisFrame();
        public bool AttackPressed => inputActions.Player.Attack.triggered;
        public bool HeavySweepPressed => inputActions.Player.HeavyAttack.triggered;
        public bool CleanHeld => inputActions.Player.Clean.IsPressed();
        public bool ToggleInvincibilityPressed => inputActions.Player.ToggleInvincibility.triggered;

        private void Awake()
        {
            inputActions = new InputSystemActions();
            inputActions.Player.Enable();
        }

        private void Update()
        {
            MoveInput = inputActions.Player.Move.ReadValue<Vector2>().x;
            VerticalInput = inputActions.Player.Move.ReadValue<Vector2>().y;
            
            if (Gamepad.current != null)
            {
                if (Gamepad.current.leftShoulder.wasPressedThisFrame)
                    Debug.Log("Left Shoulder PRESSED (raw)");
        
                if (Gamepad.current.rightShoulder.wasPressedThisFrame)
                    Debug.Log("Right Shoulder PRESSED (raw)");
            
                if (inputActions.Player.Clean.triggered)
                    Debug.Log("Clean action TRIGGERED");
            
                if (inputActions.Player.Attack.triggered)
                    Debug.Log("Attack action TRIGGERED");
                    
                if (inputActions.Player.HeavyAttack.triggered)
                    Debug.Log("HeavySweep action TRIGGERED");

                if (inputActions.Player.ToggleInvincibility.triggered)
                    Debug.Log("ToggleInvincibility action TRIGGERED");
            }
        }

        private void OnDestroy()
        {
            inputActions?.Player.Disable();
            inputActions?.Dispose();
        }
    }
}