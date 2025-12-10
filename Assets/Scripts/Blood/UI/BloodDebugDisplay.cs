using UnityEngine;
using UnityEngine.InputSystem;

namespace Blood.UI
{
    /// <summary>
    /// Debug overlay showing blood statistics similar to Minecraft's F3 menu.
    /// Toggle with F3 key (configurable in Input System).
    /// </summary>
    public class BloodDebugDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerData player;
    
        [Header("Display Settings")]
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.5f);
    
        // Input Action
        private InputAction toggleDebugAction;
    
        // State
        private bool isVisible = false;
        private GUIStyle textStyle;
        private GUIStyle backgroundStyle;
        private Texture2D backgroundTexture;
    
        // Cached values for performance
        private float updateInterval = 0.1f; // Update stats every 100ms
        private float lastUpdateTime;
        private string cachedDebugText;

        void Awake()
        {
            // Create input action for F3 toggle
            toggleDebugAction = new InputAction(
                name: "ToggleDebug",
                binding: "<Keyboard>/f3"
            );
        
            toggleDebugAction.performed += OnToggleDebug;
        
            isVisible = showOnStart;
        }

        void OnEnable()
        {
            toggleDebugAction?.Enable();
        }

        void OnDisable()
        {
            toggleDebugAction?.Disable();
        }

        void OnDestroy()
        {
            toggleDebugAction?.Dispose();
        }

        void Start()
        {
            InitializeStyles();
        
            if (player == null)
            {

                if (PlayerData.Instance != null)
                {
                    player = PlayerData.Instance;
                }

                
                if (player == null)
                {
                    Debug.LogWarning("BloodDebugDisplay: No PlayerScript found in scene!");
                }
            }
        
            UpdateDebugText();
        }

        void Update()
        {
            if (!isVisible) return;
        
            // Update cached text periodically instead of every frame
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateDebugText();
                lastUpdateTime = Time.time;
            }
        }

        void OnToggleDebug(InputAction.CallbackContext context)
        {
            isVisible = !isVisible;
            Debug.Log($"Blood Debug Display: {(isVisible ? "ON" : "OFF")}");
        }

        void InitializeStyles()
        {
            // Create background texture
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, backgroundColor);
            backgroundTexture.Apply();

            // Setup text style
            textStyle = new GUIStyle();
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = textColor;
            textStyle.padding = new RectOffset(10, 10, 10, 10);
            textStyle.fontStyle = FontStyle.Normal;
            textStyle.alignment = TextAnchor.UpperLeft;

            // Setup background style
            backgroundStyle = new GUIStyle();
            backgroundStyle.normal.background = backgroundTexture;
        }

        void UpdateDebugText()
        {
            if (BloodSystem.Instance == null)
            {
                cachedDebugText = "=== BLOOD DEBUG ===\n\nBloodSystem: NOT FOUND\n\nPress F3 to toggle this display";
                return;
            }

            // Get player position
            Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;
        
            // Gather blood statistics
            float totalBlood = BloodSystem.Instance.GetTotalBlood();
            float bloodPercentage = BloodSystem.Instance.GetBloodPercentage();
            int bloodyCells = BloodSystem.Instance.GetBloodyCellCount();
            float bloodAtPlayer = BloodSystem.Instance.GetBloodAtPosition(playerPos);
            float bloodInCleanRadius = player != null ? 
                BloodSystem.Instance.GetBloodInRadius(playerPos, 2f) : 0f;

            // Build debug string
            cachedDebugText = $"=== BLOOD DEBUG DISPLAY ===\n\n";
        
            // System info
            cachedDebugText += $"<b>System Info:</b>\n";
            cachedDebugText += $"  FPS: {(1f / Time.deltaTime):F0}\n";
            cachedDebugText += $"  Frame: {Time.frameCount}\n";
            cachedDebugText += $"  Time: {Time.time:F2}s\n\n";
        
            // Blood statistics
            cachedDebugText += $"<b>Blood Statistics:</b>\n";
            cachedDebugText += $"  Total Blood: {totalBlood:F2}\n";
            cachedDebugText += $"  Coverage: {bloodPercentage:F2}%\n";
            cachedDebugText += $"  Bloody Cells: {bloodyCells}\n";
            cachedDebugText += $"  Blood at Player: {bloodAtPlayer:F3}\n";
            cachedDebugText += $"  Blood in Clean Radius: {bloodInCleanRadius:F3}\n\n";
        
            // Player info
            if (player != null)
            {
                cachedDebugText += $"<b>Player Info:</b>\n";
                cachedDebugText += $"  Position: ({playerPos.x:F2}, {playerPos.y:F2})\n";
                cachedDebugText += $"  Cleaning: {(player.IsCleaning() ? "YES" : "NO")}\n";
                cachedDebugText += $"  Clean Radius: 2.0\n\n";
            }
        
            cachedDebugText += $"<color=#888888>Press F3 to toggle this display</color>";
        }

        void OnGUI()
        {
            if (!isVisible) return;

            // Ensure styles are initialized
            if (textStyle == null || backgroundStyle == null)
            {
                InitializeStyles();
            }

            // Calculate content size
            GUIContent content = new GUIContent(cachedDebugText);
            Vector2 size = textStyle.CalcSize(content);
        
            // Add padding
            float width = size.x + 20;
            float height = size.y + 20;
        
            // Position in top-left corner
            Rect backgroundRect = new Rect(10, 10, width, height);
            Rect textRect = new Rect(10, 10, width, height);

            // Draw background
            GUI.Box(backgroundRect, "", backgroundStyle);
        
            // Draw text
            GUI.Label(textRect, cachedDebugText, textStyle);
        }
    }
}
