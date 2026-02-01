using Inputs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace UI
{
    public class DynamicInputPrompt : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Image promptImage;

        [SerializeField] private InputIconDatabase iconDatabase;

        public enum ActionType
        {
            Move,
            Jump,
            Clean,
            Jab,
            HeavySweep,
            ToggleInvincibility
        }

        [Header("Action Selection")] [SerializeField]
        private ActionType actionToDisplay;

        [Header("Optional Settings")] [SerializeField]
        private bool hideIfNoIcon = true;

        private InputSystemActions inputActions;
        private InputAction currentAction;
        private InputDevice lastUsedDevice;

        // Static to share across all prompt instances
        private static InputDevice globalLastUsedDevice;
        private static event System.Action<InputDevice> OnDeviceChanged;

        void OnEnable()
        {
            inputActions = new InputSystemActions();
            inputActions.Enable();

            currentAction = GetActionFromType(actionToDisplay);

            // Subscribe to device change
            OnDeviceChanged += HandleDeviceChanged;

            // Set initial device
            if (globalLastUsedDevice != null)
            {
                lastUsedDevice = globalLastUsedDevice;
            }
            else
            {
                // Default to keyboard on first load
                lastUsedDevice = Keyboard.current ?? (InputDevice)Mouse.current;
                globalLastUsedDevice = lastUsedDevice;
            }

            UpdatePrompt();

            // Listen to ALL input events - this is the key!
            InputSystem.onEvent += OnInputEvent;
        }

        void OnDisable()
        {
            OnDeviceChanged -= HandleDeviceChanged;
            InputSystem.onEvent -= OnInputEvent;
            inputActions?.Disable();
            inputActions?.Dispose();
        }

        // This fires for EVERY input event from ANY device
        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            // Ignore if not a state event (actual button/axis input)
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;

            // Check if device type changed
            bool isNewDeviceType = false;

            if (device is Gamepad && !(globalLastUsedDevice is Gamepad))
            {
                isNewDeviceType = true;
            }
            else if ((device is Keyboard || device is Mouse) && globalLastUsedDevice is Gamepad)
            {
                isNewDeviceType = true;
            }

            if (isNewDeviceType)
            {
                globalLastUsedDevice = device;
                OnDeviceChanged?.Invoke(device);
            }
        }

        private void HandleDeviceChanged(InputDevice device)
        {
            lastUsedDevice = device;
            UpdatePrompt();
        }

        private InputAction GetActionFromType(ActionType type)
        {
            switch (type)
            {
                case ActionType.Jump:
                    return inputActions.Player.Jump;
                case ActionType.Jab:
                    return inputActions.Player.Attack;
                case ActionType.HeavySweep:
                    return inputActions.Player.HeavyAttack;
                case ActionType.Move:
                    return inputActions.Player.Move;
                case ActionType.Clean:
                    return inputActions.Player.Clean;
                case ActionType.ToggleInvincibility:
                    return inputActions.Player.ToggleInvincibility;
                default:
                    Debug.LogWarning($"Action type {type} not mapped in GetActionFromType!");
                    return null;
            }
        }

        private void UpdatePrompt()
        {
            if (currentAction == null || promptImage == null || iconDatabase == null)
            {
                if (hideIfNoIcon && promptImage != null)
                {
                    promptImage.enabled = false;
                }

                return;
            }

            string controlScheme;
            string deviceName = "";

            if (lastUsedDevice is Gamepad gamepad)
            {
                controlScheme = "Gamepad";
                deviceName = gamepad.name.ToLower();
            }
            else
            {
                controlScheme = "Keyboard&Mouse";
                deviceName = "keyboard";
            }

            int bindingIndex = FindBestBinding(controlScheme);

            if (bindingIndex >= 0)
            {
                string bindingPath = currentAction.bindings[bindingIndex].effectivePath;
                Sprite icon;
                if (deviceName == "keyboard" && actionToDisplay == ActionType.Move)
                {
                    icon = iconDatabase.GetIcon("move", controlScheme, deviceName);
                }
                else
                {
                    icon = iconDatabase.GetIcon(bindingPath, controlScheme, deviceName);
                }

                if (icon != null)
                {
                    promptImage.sprite = icon;
                    promptImage.enabled = true;
                }
                else
                {
                    if (hideIfNoIcon)
                    {
                        promptImage.enabled = false;
                    }

                    Debug.LogWarning(
                        $"No icon found for: {actionToDisplay} (binding: {bindingPath}, scheme: {controlScheme})");
                }
            }
            else
            {
                if (hideIfNoIcon)
                {
                    promptImage.enabled = false;
                }

                Debug.LogWarning($"No binding found for action: {actionToDisplay} in scheme: {controlScheme}");
            }
        }

        private int FindBestBinding(string controlScheme)
        {
            bool isGamepad = controlScheme == "Gamepad";
            int firstMatchingBinding = -1;
            int firstMatchingComposite = -1;

            for (int i = 0; i < currentAction.bindings.Count; i++)
            {
                var binding = currentAction.bindings[i];

                bool isGamepadBinding = binding.effectivePath.Contains("Gamepad");
                bool isKeyboardMouseBinding = binding.effectivePath.Contains("Keyboard") ||
                                              binding.effectivePath.Contains("Mouse");

                bool matches = (isGamepad && isGamepadBinding) ||
                               (!isGamepad && isKeyboardMouseBinding);

                if (!matches)
                    continue;

                // If it's a composite binding (like "WASD"), remember it
                if (binding.isComposite)
                {
                    if (firstMatchingComposite < 0)
                    {
                        firstMatchingComposite = i;
                    }

                    continue;
                }

                // If this is part of a composite
                if (binding.isPartOfComposite)
                {
                    if (firstMatchingBinding < 0)
                    {
                        firstMatchingBinding = i;
                    }

                    continue;
                }

                // Regular binding - return immediately (highest priority)
                if (!binding.isPartOfComposite && !binding.isComposite)
                {
                    return i;
                }
            }

            // For movement actions, prefer showing the composite over individual parts
            if (firstMatchingComposite >= 0)
            {
                return firstMatchingComposite;
            }

            // Otherwise return the first part of a composite if we found one
            return firstMatchingBinding;
        }

        public void SetAction(ActionType newAction)
        {
            actionToDisplay = newAction;
            currentAction = GetActionFromType(newAction);
            UpdatePrompt();
        }

        public void ForceUpdate()
        {
            UpdatePrompt();
        }
    }
}