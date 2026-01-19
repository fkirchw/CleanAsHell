using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InputIconDatabase", menuName = "Input/Icon Database")]
public class InputIconDatabase : ScriptableObject
{
    [System.Serializable]
    public class SpriteSheetReference
    {
        public Texture2D spriteSheet;
        public Sprite[] sprites;  // Auto-populated by editor script
        private Dictionary<string, Sprite> spriteCache;
        
        public Sprite GetSprite(string spriteName)
        {
            if (sprites == null || sprites.Length == 0) return null;
            
            // Build cache on first access
            if (spriteCache == null)
            {
                spriteCache = new Dictionary<string, Sprite>();
                
                foreach (Sprite sprite in sprites)
                {
                    if (sprite != null)
                    {
                        spriteCache[sprite.name] = sprite;
                    }
                }
            }
            
            spriteCache.TryGetValue(spriteName, out Sprite result);
            return result;
        }
    }
    
    [Header("Sprite Sheets")]
    public SpriteSheetReference keyboardMouseSheet;
    public SpriteSheetReference xboxSheet;
    public SpriteSheetReference playstationSheet;
    public SpriteSheetReference steamDeckSheet;
    
    // Binding path to sprite name mappings
    private static readonly Dictionary<string, string> keyboardMappings = new Dictionary<string, string>
    {
        // Special composite bindings - MUST be checked before individual keys
        // Note: Kenney pack doesn't have WASD composite, using arrows as placeholder
        { "move", "keyboard_arrows" },  // Shows 4-direction movement icon
        
        // Letters
        { "/w", "keyboard_w" },
        { "/a", "keyboard_a" },
        { "/s", "keyboard_s" },
        { "/d", "keyboard_d" },
        { "/e", "keyboard_e" },
        { "/q", "keyboard_q" },
        { "/r", "keyboard_r" },
        { "/f", "keyboard_f" },
        { "/c", "keyboard_c" },
        { "/x", "keyboard_x" },
        { "/z", "keyboard_z" },
        
        // Modifiers
        { "space", "keyboard_space" },
        { "shift", "keyboard_shift" },
        { "leftShift", "keyboard_shift" },
        { "rightShift", "keyboard_shift" },
        { "ctrl", "keyboard_ctrl" },
        { "leftCtrl", "keyboard_ctrl" },
        { "rightCtrl", "keyboard_ctrl" },
        { "alt", "keyboard_alt" },
        { "tab", "keyboard_tab" },
        { "escape", "keyboard_escape" },
        { "enter", "keyboard_enter" },
        { "backspace", "keyboard_backspace" },
        
        // Numbers
        { "/1", "keyboard_1" },
        { "/2", "keyboard_2" },
        { "/3", "keyboard_3" },
        { "/4", "keyboard_4" },
        { "/5", "keyboard_5" },
        
        // Mouse
        { "leftButton", "mouse_left" },
        { "rightButton", "mouse_right" },
        { "middleButton", "mouse_scroll" },
        
        // Arrow keys
        { "upArrow", "keyboard_arrow_up" },
        { "downArrow", "keyboard_arrow_down" },
        { "leftArrow", "keyboard_arrow_left" },
        { "rightArrow", "keyboard_arrow_right" },
    };
    
    // Xbox controller mappings - based on XML sprite names
    private static readonly Dictionary<string, string> xboxMappings = new Dictionary<string, string>
    {
        // Face buttons
        { "buttonSouth", "xbox_button_a" },       // A button (green)
        { "buttonEast", "xbox_button_b" },        // B button (red)
        { "buttonWest", "xbox_button_x" },        // X button (blue)
        { "buttonNorth", "xbox_button_y" },       // Y button (yellow)
        
        // Shoulders and triggers
        { "leftShoulder", "xbox_lb" },            // LB
        { "rightShoulder", "xbox_rb" },           // RB
        { "leftTrigger", "xbox_lt" },             // LT
        { "rightTrigger", "xbox_rt" },            // RT
        
        // D-Pad
        { "dpad/up", "xbox_dpad_up" },
        { "dpad/down", "xbox_dpad_down" },
        { "dpad/left", "xbox_dpad_left" },
        { "dpad/right", "xbox_dpad_right" },
        { "dpad", "xbox_dpad" },
        
        // Sticks
        { "leftStick", "xbox_stick_l" },
        { "rightStick", "xbox_stick_r" },
        { "leftStickPress", "xbox_ls" },          // L3 equivalent
        { "rightStickPress", "xbox_rs" },         // R3 equivalent
        
        // System buttons
        { "start", "xbox_button_start" },
        { "select", "xbox_button_back" },
    };
    
    // PlayStation controller mappings - based on XML sprite names
    private static readonly Dictionary<string, string> playstationMappings = new Dictionary<string, string>
    {
        // Face buttons
        { "buttonSouth", "playstation_button_cross" },      // Cross/X
        { "buttonEast", "playstation_button_circle" },      // Circle
        { "buttonWest", "playstation_button_square" },      // Square
        { "buttonNorth", "playstation_button_triangle" },   // Triangle
        
        // Shoulders and triggers
        { "leftShoulder", "playstation_trigger_l1" },       // L1
        { "rightShoulder", "playstation_trigger_r1" },      // R1
        { "leftTrigger", "playstation_trigger_l2" },        // L2
        { "rightTrigger", "playstation_trigger_r2" },       // R2
        
        // D-Pad
        { "dpad/up", "playstation_dpad_up" },
        { "dpad/down", "playstation_dpad_down" },
        { "dpad/left", "playstation_dpad_left" },
        { "dpad/right", "playstation_dpad_right" },
        { "dpad", "playstation_dpad" },
        
        // Sticks
        { "leftStick", "playstation_stick_l" },
        { "rightStick", "playstation_stick_r" },
        { "leftStickPress", "playstation_button_l3" },      // L3
        { "rightStickPress", "playstation_button_r3" },     // R3
        
        // System buttons
        { "start", "playstation4_button_options" },         // Options button
        { "select", "playstation4_button_share" },          // Share button
    };
    
    public Sprite GetIcon(string bindingPath, string controlScheme, string deviceName)
    {
        if (string.IsNullOrEmpty(bindingPath))
        {
            Debug.LogWarning("Binding path is null or empty!");
            return null;
        }
        
        // Determine which sheet and mappings to use
        SpriteSheetReference sheet = null;
        Dictionary<string, string> mappings = null;
        
        if (controlScheme == "Keyboard&Mouse")
        {
            sheet = keyboardMouseSheet;
            mappings = keyboardMappings;
        }
        else if (controlScheme == "Gamepad")
        {
            // Check specific device type and use appropriate mappings
            if (deviceName.Contains("xbox") || deviceName.Contains("xinput"))
            {
                sheet = xboxSheet;
                mappings = xboxMappings;  // Use Xbox sprite names
            }
            else if (deviceName.Contains("playstation") || deviceName.Contains("dualshock") || deviceName.Contains("ps"))
            {
                sheet = playstationSheet;
                mappings = playstationMappings;  // Use PlayStation sprite names
            }
            else if (deviceName.Contains("steamdeck") || deviceName.Contains("steam"))
            {
                sheet = steamDeckSheet;
                mappings = xboxMappings;  // Steam Deck uses Xbox-style naming
            }
            else
            {
                // Default to Xbox for unknown gamepads
                sheet = xboxSheet;
                mappings = xboxMappings;
            }
        }
        
        if (sheet == null || sheet.sprites == null || sheet.sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites assigned for scheme: {controlScheme}, device: {deviceName}");
            return null;
        }
        
        // Find matching sprite name
        string spriteName = null;
        foreach (var kvp in mappings)
        {
            if (bindingPath.EndsWith(kvp.Key))
            {
                spriteName = kvp.Value;
                break;
            }
        }
        
        if (string.IsNullOrEmpty(spriteName))
        {
            Debug.LogWarning($"No sprite mapping found for binding path: {bindingPath}");
            return null;
        }
        
        Sprite sprite = sheet.GetSprite(spriteName);
        
        if (sprite == null)
        {
            Debug.LogWarning($"Sprite '{spriteName}' not found in sprite sheet");
        }
        
        return sprite;
    }
}