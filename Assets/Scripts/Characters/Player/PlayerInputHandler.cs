using Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.Player
{
    /// <summary>
    /// Central hub for all player input. Other player scripts query this for input state.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        private InputSystemActions inputActions;

        // Public properties - other scripts read these
        public float MoveInput { get; private set; }
        public bool JumpPressed => inputActions.Player.Jump.triggered;
        public bool JumpHeld => inputActions.Player.Jump.IsPressed();
        public bool JumpReleased => inputActions.Player.Jump.WasReleasedThisFrame();
        public bool AttackPressed => inputActions.Player.Attack.triggered;
        public bool HeavySweepPressed => inputActions.Player.HeavyAttack.triggered;
        public bool CleanHeld => inputActions.Player.Clean.IsPressed();

        private void Awake()
        {
            inputActions = new InputSystemActions();
            inputActions.Player.Enable();
        }

        private void Update()
        {
            // Update continuous input each frame
            MoveInput = inputActions.Player.Move.ReadValue<Vector2>().x;
            
            // Temporary debug - remove after fixing
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
            }
        }

        private void OnDestroy()
        {
            inputActions?.Player.Disable();
            inputActions?.Dispose();
        }
    }
}