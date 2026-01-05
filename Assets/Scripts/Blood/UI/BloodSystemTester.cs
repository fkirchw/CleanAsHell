using UnityEngine;
using UnityEngine.InputSystem;

namespace Blood.UI
{
    /// <summary>
    /// Test script to verify blood system functionality.
    /// Attach to any GameObject to add test controls.
    /// 
    /// Controls:
    /// - T: Add blood at mouse position
    /// - C: Clean blood at mouse position  
    /// - R: Reset all blood
    /// - B: Print blood statistics
    /// </summary>
    public class BloodSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private float testBloodAmount = 0.5f;
        [SerializeField] private float testCleanAmount = 0.3f;
        [SerializeField] private float testRadius = 2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Visual Feedback")]
        [SerializeField] private bool showMouseGizmo = true;
        [SerializeField] private Color gizmoColor = Color.cyan;

        private Camera mainCamera;
        private Vector2 mouseWorldPos;
        
        // Input actions created at runtime
        private InputAction addBloodAction;
        private InputAction cleanBloodAction;
        private InputAction cleanContinuousAction;
        private InputAction resetBloodAction;
        private InputAction printStatsAction;
        private InputAction mousePositionAction;

        void Start()
        {
            mainCamera = Camera.main;
        
            SetupInputActions();
        
            if (BloodSystem.Instance == null)
            {
                Debug.LogError("BloodSystemTester: BloodSystem.Instance is NULL! Make sure BloodSystem exists in scene.");
            }
            else
            {
                Debug.Log("BloodSystemTester initialized. Controls:\n" +
                          "T - Add blood at mouse\n" +
                          "C - Clean blood at mouse\n" +
                          "R - Reset all blood\n" +
                          "B - Print blood stats");
            }
        }

        private void SetupInputActions()
        {
            // Create actions programmatically
            addBloodAction = new InputAction("AddBlood", binding: "<Keyboard>/t");
            cleanBloodAction = new InputAction("CleanBlood", binding: "<Keyboard>/c", type: InputActionType.Button);
            cleanContinuousAction = new InputAction("CleanContinuous", binding: "<Keyboard>/c", type: InputActionType.Button);
            resetBloodAction = new InputAction("ResetBlood", binding: "<Keyboard>/r");
            printStatsAction = new InputAction("PrintStats", binding: "<Keyboard>/b");
            mousePositionAction = new InputAction("MousePosition", binding: "<Mouse>/position");
            
            // Enable all actions
            addBloodAction.Enable();
            cleanBloodAction.Enable();
            cleanContinuousAction.Enable();
            resetBloodAction.Enable();
            printStatsAction.Enable();
            mousePositionAction.Enable();
        }

        void Update()
        {
            // Update mouse position
            if (mainCamera)
            {
                Vector2 mouseScreenPos = mousePositionAction.ReadValue<Vector2>();
                mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            }

            // Test controls
            if (addBloodAction.triggered)
            {
                TestAddBlood();
            }

            if (cleanBloodAction.triggered)
            {
                TestCleanBlood();
            }

            if (resetBloodAction.triggered)
            {
                TestResetBlood();
            }

            if (printStatsAction.triggered)
            {
                PrintBloodStats();
            }

            // Hold C for continuous cleaning
            if (cleanContinuousAction.IsPressed())
            {
                if (BloodSystem.Instance != null)
                {
                    BloodSystem.Instance.CleanBlood(mouseWorldPos, testRadius, testCleanAmount * Time.deltaTime);
                }
            }
        }

        void TestAddBlood()
        {
            if (!BloodSystem.Instance)
            {
                Debug.LogError("Cannot test - BloodSystem.Instance is null!");
                return;
            }

            // Simulate a hit
            Vector2 strikeDirection = Random.insideUnitCircle.normalized;
            BloodSystem.Instance.OnEnemyHit(mouseWorldPos, strikeDirection, false, testBloodAmount);
        
            Debug.Log($"Added blood at {mouseWorldPos} (amount: {testBloodAmount})");
        }

        void TestCleanBlood()
        {
            if (!BloodSystem.Instance)
            {
                Debug.LogError("Cannot test - BloodSystem.Instance is null!");
                return;
            }

            float bloodBefore = BloodSystem.Instance.GetBloodInRadius(mouseWorldPos, testRadius);
            BloodSystem.Instance.CleanBlood(mouseWorldPos, testRadius, testCleanAmount);
            float bloodAfter = BloodSystem.Instance.GetBloodInRadius(mouseWorldPos, testRadius);
        
            Debug.Log($"Cleaned blood at {mouseWorldPos}\n" +
                      $"  Before: {bloodBefore:F3}\n" +
                      $"  After: {bloodAfter:F3}\n" +
                      $"  Removed: {bloodBefore - bloodAfter:F3}");
        }

        void TestResetBlood()
        {
            if (!BloodSystem.Instance)
            {
                Debug.LogError("Cannot test - BloodSystem.Instance is null!");
                return;
            }

            float totalBefore = BloodSystem.Instance.GetTotalBlood();
        
            // Clean the entire map by cleaning a huge area
            BloodSystem.Instance.CleanBlood(Vector2.zero, 1000f, 999f);
        
            float totalAfter = BloodSystem.Instance.GetTotalBlood();
        
            Debug.Log($"Reset blood system\n" +
                      $"  Before: {totalBefore:F2}\n" +
                      $"  After: {totalAfter:F2}");
        }

        void PrintBloodStats()
        {
            if (!BloodSystem.Instance)
            {
                Debug.LogError("Cannot print stats - BloodSystem.Instance is null!");
                return;
            }

            float totalBlood = BloodSystem.Instance.GetTotalBlood();
            float percentage = BloodSystem.Instance.GetBloodPercentage();
            int bloodyCells = BloodSystem.Instance.GetBloodyCellCount();
            float bloodAtMouse = BloodSystem.Instance.GetBloodAtPosition(mouseWorldPos);
            float bloodInRadius = BloodSystem.Instance.GetBloodInRadius(mouseWorldPos, testRadius);

            Debug.Log("=== BLOOD STATISTICS ===\n" +
                      $"Total Blood: {totalBlood:F2}\n" +
                      $"Coverage: {percentage:F2}%\n" +
                      $"Bloody Cells: {bloodyCells}\n" +
                      $"Blood at Mouse: {bloodAtMouse:F3}\n" +
                      $"Blood in Radius: {bloodInRadius:F3}");
        }

        void OnDrawGizmos()
        {
            if (!showMouseGizmo || !mainCamera) return;

            // Draw circle at mouse position
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(mouseWorldPos, testRadius);

            // Draw crosshair at exact mouse position
            Gizmos.DrawLine(mouseWorldPos + Vector2.left * 0.2f, mouseWorldPos + Vector2.right * 0.2f);
            Gizmos.DrawLine(mouseWorldPos + Vector2.down * 0.2f, mouseWorldPos + Vector2.up * 0.2f);
        }

        private void OnDestroy()
        {
            // Clean up actions
            addBloodAction?.Disable();
            cleanBloodAction?.Disable();
            cleanContinuousAction?.Disable();
            resetBloodAction?.Disable();
            printStatsAction?.Disable();
            mousePositionAction?.Disable();
            
            addBloodAction?.Dispose();
            cleanBloodAction?.Dispose();
            cleanContinuousAction?.Dispose();
            resetBloodAction?.Dispose();
            printStatsAction?.Dispose();
            mousePositionAction?.Dispose();
        }
    }
}