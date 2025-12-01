using UnityEngine;

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

        void Start()
        {
            mainCamera = Camera.main;
        
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

        void Update()
        {
            // Update mouse position
            if (mainCamera != null)
            {
                mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            }

            // Test controls
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestAddBlood();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                TestCleanBlood();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TestResetBlood();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                PrintBloodStats();
            }

            // Hold C for continuous cleaning
            if (Input.GetKey(KeyCode.C))
            {
                if (BloodSystem.Instance != null)
                {
                    BloodSystem.Instance.CleanBlood(mouseWorldPos, testRadius, testCleanAmount * Time.deltaTime);
                }
            }
        }

        void TestAddBlood()
        {
            if (BloodSystem.Instance == null)
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
            if (BloodSystem.Instance == null)
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
            if (BloodSystem.Instance == null)
            {
                Debug.LogError("Cannot test - BloodSystem.Instance is null!");
                return;
            }

            // This will need a public method in BloodSystem or we can clean everything
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
            if (BloodSystem.Instance == null)
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
            if (!showMouseGizmo || mainCamera == null) return;

            // Draw circle at mouse position
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(mouseWorldPos, testRadius);

            // Draw crosshair at exact mouse position
            Gizmos.DrawLine(mouseWorldPos + Vector2.left * 0.2f, mouseWorldPos + Vector2.right * 0.2f);
            Gizmos.DrawLine(mouseWorldPos + Vector2.down * 0.2f, mouseWorldPos + Vector2.up * 0.2f);
        }
    }
}
