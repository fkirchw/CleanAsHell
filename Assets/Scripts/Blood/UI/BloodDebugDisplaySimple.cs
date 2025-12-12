using Characters.Player;
using UnityEngine;

namespace Blood.UI
{
    /// <summary>
    /// Debug overlay showing blood statistics similar to Minecraft's F3 menu.
    /// Toggle with F3 key (legacy input system).
    /// This version uses Input.GetKeyDown for easier setup without Input System package.
    /// </summary>
    public class BloodDebugDisplaySimple : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerData player;
    
        [Header("Display Settings")]
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.5f);
    
        // State
        private bool isVisible = false;
        private GUIStyle textStyle;
        private GUIStyle backgroundStyle;
        private Texture2D backgroundTexture;
    
        // Cached values for performance
        private float updateInterval = 0.1f;
        private float lastUpdateTime;
        private string cachedDebugText;

        public void Awake()
        {
            player = GetComponent<PlayerData>();
        }


        void Start()
        {
            
            if (player == null)
            {
                Debug.LogWarning("BloodDebugDisplay: No PlayerScript found in scene!");
                return;
            }

            InitializeStyles();
            isVisible = showOnStart;

            UpdateDebugText();
        }

        void Update()
        {
            // Toggle visibility with F3
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
                Debug.Log($"Blood Debug Display: {(isVisible ? "ON" : "OFF")}");
            }
        
            if (!isVisible) return;
        
            // Update cached text periodically instead of every frame
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateDebugText();
                lastUpdateTime = Time.time;
            }
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
            textStyle.richText = true; // Enable rich text for colors/bold

            // Setup background style
            backgroundStyle = new GUIStyle();
            backgroundStyle.normal.background = backgroundTexture;
        }

        void UpdateDebugText()
        {
            if (!BloodSystem.Instance)
            {
                cachedDebugText = "=== BLOOD DEBUG ===\n\n" +
                                  "BloodSystem: <color=red>NOT FOUND</color>\n\n" +
                                  $"Press {toggleKey} to toggle this display";
                return;
            }

            // Get player position
            Vector3 playerPos = player ? player.transform.position : Vector3.zero;
        
            // Gather blood statistics
            float totalBlood = BloodSystem.Instance.GetTotalBlood();
            float bloodPercentage = BloodSystem.Instance.GetBloodPercentage();
            int bloodyCells = BloodSystem.Instance.GetBloodyCellCount();
            float bloodAtPlayer = BloodSystem.Instance.GetBloodAtPosition(playerPos);
            float bloodInCleanRadius = player ? 
                BloodSystem.Instance.GetBloodInRadius(playerPos, 2f) : 0f;

            // Build debug string with rich text
            cachedDebugText = "<b>=== BLOOD DEBUG DISPLAY ===</b>\n\n";
        
            // System info
            cachedDebugText += "<b>System Info:</b>\n";
            cachedDebugText += $"  FPS: {(1f / Time.smoothDeltaTime):F0}\n";
            cachedDebugText += $"  Frame: {Time.frameCount}\n";
            cachedDebugText += $"  Time: {Time.time:F2}s\n";
            cachedDebugText += $"  Delta: {Time.deltaTime * 1000:F1}ms\n\n";
        
            // Blood statistics
            cachedDebugText += "<b>Blood Statistics:</b>\n";
            cachedDebugText += $"  Total Blood: <color=yellow>{totalBlood:F2}</color>\n";
            cachedDebugText += $"  Coverage: <color=yellow>{bloodPercentage:F2}%</color>\n";
            cachedDebugText += $"  Bloody Cells: <color=yellow>{bloodyCells}</color>\n";
            cachedDebugText += $"  Blood at Player: <color=yellow>{bloodAtPlayer:F3}</color>\n";
            cachedDebugText += $"  Blood in Clean Radius: <color=yellow>{bloodInCleanRadius:F3}</color>\n\n";
        
            // Player info
            if (player)
            {
                cachedDebugText += "<b>Player Info:</b>\n";
                cachedDebugText += $"  Position: <color=cyan>({playerPos.x:F2}, {playerPos.y:F2})</color>\n";
            
                bool cleaning = player.IsCleaning;
                string cleaningColor = cleaning ? "lime" : "gray";
                cachedDebugText += $"  Cleaning: <color={cleaningColor}>{(cleaning ? "YES" : "NO")}</color>\n";
                cachedDebugText += $"  Clean Radius: <color=cyan>2.0</color>\n\n";
            }
        
            cachedDebugText += $"<color=#888888>Press {toggleKey} to toggle • Update: {updateInterval:F1}s</color>";
        }

        void OnGUI()
        {
            if (!isVisible) return;

            // Ensure styles are initialized (can be null after scene reload)
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
