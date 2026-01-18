using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Blood
{
    /// <summary>
    /// Advanced blood system with spreading, dripping, and cleaning mechanics.
    /// Uses a single tilemap with shader-based overlay for efficient rendering.
    /// </summary>
    public class BloodSystem : MonoBehaviour
    {
        private static readonly int BloodMask = Shader.PropertyToID("_BloodMask");
        private static readonly int BloodTexture = Shader.PropertyToID("_BloodTexture");
        private static readonly int BloodColor = Shader.PropertyToID("_BloodColor");
        private static readonly int BloodTiling = Shader.PropertyToID("_BloodTiling");
        private static readonly int BloodMaskSt = Shader.PropertyToID("_BloodMask_ST");
        public static BloodSystem Instance { get; private set; }

        [Header("References")] [SerializeField]
        private Tilemap floorTilemap;

        [SerializeField] private Grid grid;
        [SerializeField] private ParticleSystem bloodParticles;

        [Header("Visual Settings")] [SerializeField]
        private Texture2D bloodSplatTexture;

        [SerializeField] private Color bloodColor = new Color(0.5f, 0, 0, 1);
        [SerializeField] private float logarithmicBase = 10f;
        [SerializeField] private float bloodTiling = 2f;

        [Header("Spreading Settings")] [SerializeField]
        private bool enableSpreading = true;

        [SerializeField] private float spreadRate = 0.01f;
        [SerializeField] private float spreadThreshold = 0.3f;
        [SerializeField] private float spreadUpdateInterval = 0.1f;

        [SerializeField] [Range(0.1f, 1f)]
        private float bloodRetentionFactor = 1f; // How much blood actually lands vs. is "lost"


        [Header("Surface Detection")] [SerializeField]
        private float maxBloodCapacity = 10f; // High cap, no overflow drain

        [SerializeField] private LayerMask groundLayer;

        // Internal state
        private Texture2D bloodMask;
        private Material bloodMaterial;
        private float[,] bloodData;
        private float[,] bloodDataBuffer;
        private Vector2Int gridOffset;
        private int gridWidth;
        private int gridHeight;
        private float spreadTimer;
        private bool needsVisualUpdate;
        private HashSet<Vector2Int> surfaceTiles;
        private HashSet<Vector2Int> validFloorTiles;
        private float totalBloodGenerated;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            InitializeBloodSystem();
            CacheSurfaceAndFloorTiles();
        }

        void CacheSurfaceAndFloorTiles()
        {
            surfaceTiles = new HashSet<Vector2Int>();
            validFloorTiles = new HashSet<Vector2Int>();

            BoundsInt bounds = floorTilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);

                    if (!floorTilemap.HasTile(cellPos))
                        continue;

                    // Store in array coordinates directly
                    int arrayX = x - gridOffset.x;
                    int arrayY = y - gridOffset.y;

                    if (arrayX < 0 || arrayX >= gridWidth || arrayY < 0 || arrayY >= gridHeight)
                        continue;

                    Vector2Int arrayPos = new Vector2Int(arrayX, arrayY);

                    if (IsFloorNotWall(cellPos))
                    {
                        validFloorTiles.Add(arrayPos);

                        if (IsSurfaceTile(cellPos))
                        {
                            surfaceTiles.Add(arrayPos);
                        }
                    }
                }
            }

            Debug.Log($"Found {validFloorTiles.Count} floor tiles, {surfaceTiles.Count} are on surface");
        }

        bool IsFloorNotWall(Vector3Int cellPos)
        {
            // Check if there's empty space above (not a wall)
            for (int i = 1; i <= 3; i++)
            {
                Vector3Int above = cellPos + new Vector3Int(0, i, 0);
                if (floorTilemap.HasTile(above))
                {
                    return false;
                }
            }

            return true;
        }

        bool IsSurfaceTile(Vector3Int cellPos)
        {
            // Check if there's empty space above
            for (int i = 1; i <= 2; i++)
            {
                Vector3Int above = cellPos + new Vector3Int(0, i, 0);
                if (!floorTilemap.HasTile(above))
                {
                    // At least one empty space above = it's a surface
                    return true;
                }
            }

            return false; // Completely buried
        }

        bool IsValidBloodTile(int arrayX, int arrayY)
        {
            if (arrayX < 0 || arrayX >= gridWidth || arrayY < 0 || arrayY >= gridHeight)
                return false;

            Vector2Int pos = new Vector2Int(arrayX, arrayY);
            return surfaceTiles.Contains(pos);
        }

        void Update()
        {
            if (enableSpreading)
            {
                spreadTimer += Time.deltaTime;

                if (spreadTimer >= spreadUpdateInterval)
                {
                    spreadTimer = 0f;
                    UpdateSpreading();
                    needsVisualUpdate = true;
                }
            }

            if (needsVisualUpdate)
            {
                UpdateAllVisualsOptimized();
                needsVisualUpdate = false;
            }
        }

        void InitializeBloodSystem()
        {
            BoundsInt bounds = floorTilemap.cellBounds;

            gridWidth = bounds.size.x;
            gridHeight = bounds.size.y;
            gridOffset = new Vector2Int(bounds.xMin, bounds.yMin);

            Debug.Log(
                $"[Init] Bounds: {bounds}, gridWidth: {gridWidth}, gridHeight: {gridHeight}, gridOffset: {gridOffset}");
            Debug.Log($"[Init] Max cell should be: ({bounds.xMax}, {bounds.yMax})");
            Debug.Log($"[Init] Max array index should be: ({gridWidth - 1}, {gridHeight - 1})");

            // Initialize data arrays
            bloodData = new float[gridWidth, gridHeight];
            bloodDataBuffer = new float[gridWidth, gridHeight];

            // Create blood mask texture
            bloodMask = new Texture2D(gridWidth, gridHeight, TextureFormat.RGBAFloat, false);
            bloodMask.filterMode = FilterMode.Bilinear;
            bloodMask.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[gridWidth * gridHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0, 0, 0, 0);
            }

            bloodMask.SetPixels(pixels);
            bloodMask.Apply();

            // Setup material with shader
            TilemapRenderer tilemapRenderer = floorTilemap.GetComponent<TilemapRenderer>();

            // Create new material instance from the shader
            Shader bloodShader = Shader.Find("Custom/TilemapBlood");
            if (bloodShader == null)
            {
                Debug.LogError("Blood shader not found! Make sure 'Custom/TilemapBlood' shader exists.");
                return;
            }

            bloodMaterial = new Material(bloodShader);
            bloodMaterial.SetTexture(BloodMask, bloodMask);
            bloodMaterial.SetTexture(BloodTexture, bloodSplatTexture);
            bloodMaterial.SetColor(BloodColor, bloodColor);
            bloodMaterial.SetFloat(BloodTiling, bloodTiling);

            // Calculate world space bounds of the grid
            Vector2 worldMin = floorTilemap.CellToWorld(new Vector3Int(gridOffset.x, gridOffset.y, 0));
            Vector2 worldMax = floorTilemap.CellToWorld(new Vector3Int(gridOffset.x + gridWidth, gridOffset.y + gridHeight, 0));

            // Set up the blood mask tiling/offset
            Vector2 cellHalfSize = floorTilemap.cellSize * 0.5f;

            Vector4 maskST = new Vector4(
                1f / (worldMax.x - worldMin.x), // Scale X
                1f / (worldMax.y - worldMin.y), // Scale Y
                worldMin.x + cellHalfSize.x, // Offset X - centered on cell
                worldMin.y + cellHalfSize.y // Offset Y - centered on cell
            );

            bloodMaterial.SetVector(BloodMaskSt, maskST);

            tilemapRenderer.material = bloodMaterial;

            Debug.Log($"worldMin: {worldMin}, worldMax: {worldMax}");
            Debug.Log($"maskST: {maskST}");
            Debug.Log($"Grid bounds: {bounds}");
        }

        /// <summary>
        /// Called when an enemy is hit to spawn blood
        /// </summary>
        public void OnEnemyHit(Vector2 hitPosition, Vector2 strikeDirection, bool isAirborne, float bloodAmount)
        {
            SpawnParticles(hitPosition, strikeDirection, bloodAmount);

            Vector2 stainCenter = CalculateStainCenter(hitPosition, strikeDirection, isAirborne);
            Debug.Log($"Stain center: {stainCenter}");
            float actuallyStained = StainArea(stainCenter, strikeDirection, bloodAmount);
            Debug.Log($"Actually stained: {actuallyStained}");
            // Only count blood that actually landed on valid surfaces
            totalBloodGenerated += actuallyStained;
        }

        void SpawnParticles(Vector2 position, Vector2 direction, float bloodAmount)
        {
            if (!bloodParticles) return;

            // Ensure Start Speed doesn't interfere
            var main = bloodParticles.main;
            main.startSpeed = 0f;

            int particleCount = Mathf.RoundToInt(Random.Range(5, 15) * bloodAmount);

            for (int i = 0; i < particleCount; i++)
            {
                // Create cone spread
                float coneSpread = Random.Range(-20f, 20f) * Mathf.Deg2Rad;
                float baseAngle = Mathf.Atan2(direction.y, direction.x);
                Vector3 particleVelocity = new Vector3(
                    Mathf.Cos(baseAngle + coneSpread),
                    Mathf.Sin(baseAngle + coneSpread),
                    0f
                ) * Random.Range(3f, 7f);

                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                emitParams.position = position;
                emitParams.velocity = particleVelocity;
                emitParams.startSize = Random.Range(0.1f, 0.3f);
                emitParams.startLifetime = Random.Range(0.5f, 1.5f);

                bloodParticles.Emit(emitParams, 1);
            }
        }

        Vector2 CalculateStainCenter(Vector2 hitPosition, Vector2 direction, bool isAirborne)
        {
            if (isAirborne)
            {
                RaycastHit2D hit = Physics2D.Raycast(
                    hitPosition,
                    Vector2.down,
                    100f,
                    groundLayer
                );

                if (hit.collider)
                {
                    return hit.point + direction.normalized * 0.5f;
                }

                return hitPosition + Vector2.down * 2f;
            }

            return hitPosition + direction.normalized * 0.5f;
        }

        float StainArea(Vector2 centerPosition, Vector2 direction, float bloodAmount)
        {
            Vector3Int centerCell = floorTilemap.WorldToCell(centerPosition);
            Vector2 normalizedDir = direction.normalized;

            Debug.Log($"[StainArea] World pos: {centerPosition}, Tilemap cell: {centerCell}, GridOffset: {gridOffset}");

            int stainRadius = 2;

            List<(Vector3Int cell, float weight)> validCells = new List<(Vector3Int, float)>();

            for (int x = -stainRadius; x <= stainRadius; x++)
            {
                for (int y = -stainRadius; y <= stainRadius; y++)
                {
                    Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);

                    // Convert to array coordinates
                    int arrayX = cellPos.x - gridOffset.x;
                    int arrayY = cellPos.y - gridOffset.y;

                    // Check bounds first
                    if (arrayX < 0 || arrayX >= gridWidth || arrayY < 0 || arrayY >= gridHeight)
                        continue;

                    if (!floorTilemap.HasTile(cellPos))
                        continue;

                    // Check if it's a valid blood surface using array coordinates
                    if (!IsValidBloodTile(arrayX, arrayY))
                        continue;

                    Vector2 cellWorld = floorTilemap.GetCellCenterWorld(cellPos);
                    Vector2 toCell = cellWorld - centerPosition;

                    float dotProduct = Vector2.Dot(toCell.normalized, normalizedDir);

                    if (dotProduct > 0.3f)
                    {
                        float distance = toCell.magnitude;
                        float weight = Mathf.Max(0, 1f - (distance / (stainRadius * floorTilemap.cellSize.x)));

                        if (weight > 0.01f)
                        {
                            validCells.Add((cellPos, weight));
                        }
                    }
                }
            }

            Debug.Log($"[StainArea] Found {validCells.Count} valid cells");

            if (validCells.Count == 0)
            {
                return 0f; // Blood fell off the level - doesn't count
            }

            // Calculate total weight
            float totalWeight = 0f;
            foreach (var (_, weight) in validCells)
            {
                totalWeight += weight;
            }

            // Apply retention factor - simulate blood "loss" (splatter, absorption, etc.)
            float effectiveBloodAmount = bloodAmount * bloodRetentionFactor;

            // Distribute blood proportionally - effective amount gets distributed
            foreach (var (cell, weight) in validCells)
            {
                float cellBloodAmount = effectiveBloodAmount * (weight / totalWeight);
                AddBloodAtCell(cell, cellBloodAmount);
            }

            needsVisualUpdate = true;
            return effectiveBloodAmount; // Return what actually landed
        }

        void AddBloodAtCell(Vector3Int cellPosition, float amount)
        {
            int arrayX = cellPosition.x - gridOffset.x;
            int arrayY = cellPosition.y - gridOffset.y;

            if (arrayX < 0 || arrayX >= gridWidth || arrayY < 0 || arrayY >= gridHeight)
                return;

            if (!IsValidBloodTile(arrayX, arrayY))
                return;

            bloodData[arrayX, arrayY] = Mathf.Min(maxBloodCapacity, bloodData[arrayX, arrayY] + amount);
        }

        void UpdateSpreading()
        {
            Array.Copy(bloodData, bloodDataBuffer, bloodData.Length);

            BoundsInt bounds = new BoundsInt(0, 0, 0, gridWidth, gridHeight, 1);

            foreach (var cellPos in bounds.allPositionsWithin)
            {
                int x = cellPos.x;
                int y = cellPos.y;

                float currentBlood = bloodDataBuffer[x, y];

                if (currentBlood < spreadThreshold)
                    continue;

                if (!IsValidBloodTile(x, y))
                    continue;

                float spreadAmount = currentBlood * spreadRate;

                Vector2Int[] neighbors =
                {
                    new Vector2Int(x - 1, y),
                    new Vector2Int(x + 1, y),
                    new Vector2Int(x, y - 1),
                    new Vector2Int(x, y + 1)
                };

                int validNeighbors = 0;
                foreach (Vector2Int neighbor in neighbors)
                {
                    if (IsValidBloodTile(neighbor.x, neighbor.y))
                        validNeighbors++;
                }

                if (validNeighbors == 0)
                    continue;

                float spreadPerNeighbor = spreadAmount / validNeighbors;

                foreach (Vector2Int neighbor in neighbors)
                {
                    if (IsValidBloodTile(neighbor.x, neighbor.y))
                    {
                        bloodData[neighbor.x, neighbor.y] = Mathf.Min(maxBloodCapacity,
                            bloodData[neighbor.x, neighbor.y] + spreadPerNeighbor);
                    }
                }

                bloodData[x, y] = Mathf.Max(0, bloodData[x, y] - spreadAmount);
            }
        }

        void UpdateAllVisualsOptimized()
        {
            Color[] pixels = new Color[gridWidth * gridHeight];

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    float linearBlood = bloodData[x, y];
                    float visualAlpha = BloodToVisualAlpha(linearBlood);
                    pixels[y * gridWidth + x] = new Color(visualAlpha, visualAlpha, visualAlpha, visualAlpha);
                }
            }

            bloodMask.SetPixels(pixels);
            bloodMask.Apply();
        }

        float BloodToVisualAlpha(float linearBlood)
        {
            if (linearBlood <= 0) return 0;

            float normalizedBlood = Mathf.Clamp01(linearBlood / maxBloodCapacity);
            return Mathf.Log(1f + normalizedBlood * (logarithmicBase - 1f), logarithmicBase);
        }
        // PUBLIC API FOR QUERYING AND CLEANING

        /// <summary>
        /// Clean blood in a radius around a world position
        /// </summary>
        public void CleanBlood(Vector2 worldPosition, float radius, float cleanAmount)
        {
            Vector3Int centerCell = floorTilemap.WorldToCell(worldPosition);
            int radiusCells = Mathf.CeilToInt(radius / floorTilemap.cellSize.x);

            for (int x = -radiusCells; x <= radiusCells; x++)
            {
                for (int y = -radiusCells; y <= radiusCells; y++)
                {
                    Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);

                    int arrayX = cellPos.x - gridOffset.x;
                    int arrayY = cellPos.y - gridOffset.y;

                    if (arrayX < 0 || arrayX >= gridWidth || arrayY < 0 || arrayY >= gridHeight)
                        continue;

                    Vector2 cellCenter = floorTilemap.CellToWorld(cellPos);
                    if (Vector2.Distance(worldPosition, cellCenter) > radius)
                        continue;

                    bloodData[arrayX, arrayY] = Mathf.Max(0, bloodData[arrayX, arrayY] - cleanAmount);
                }
            }

            needsVisualUpdate = true;
        }

        public void CleanBloodOnGround(Vector2 worldPosition, float groundY, float verticalTolerance,
            float horizontalRadius, float cleanAmount)
        {
            // Convert horizontal radius to grid cells
            int radiusCells = Mathf.CeilToInt(horizontalRadius / floorTilemap.cellSize.x);

            // Convert vertical tolerance to grid cells
            int verticalCells = Mathf.Max(1, Mathf.CeilToInt(verticalTolerance / floorTilemap.cellSize.y));

            // Find the center cell based on player position
            Vector3Int centerCell = floorTilemap.WorldToCell(worldPosition);

            // Find the ground cell Y coordinate
            Vector3Int groundCell = floorTilemap.WorldToCell(new Vector2(worldPosition.x, groundY));
            int targetY = groundCell.y;

            int cleanedCount = 0;

            // Only iterate horizontally around the player
            for (int x = -radiusCells; x <= radiusCells; x++)
            {
                // Only check cells near the ground height (with tolerance)
                for (int yOffset = -verticalCells; yOffset <= verticalCells; yOffset++)
                {
                    Vector3Int cellPos = new Vector3Int(
                        centerCell.x + x,
                        targetY + yOffset, // Use ground Y, not player Y
                        0
                    );

                    int arrayX = cellPos.x - gridOffset.x;
                    int arrayY = cellPos.y - gridOffset.y;

                    if (arrayX < 0 || arrayX >= gridWidth || arrayY < 0 || arrayY >= gridHeight)
                        continue;

                    // Check if this cell is within horizontal radius
                    Vector2 cellCenter = floorTilemap.CellToWorld(cellPos);
                    float horizontalDist = Mathf.Abs(cellCenter.x - worldPosition.x);

                    if (horizontalDist > horizontalRadius)
                        continue;

                    // Check if cell is within vertical tolerance of ground
                    float verticalDist = Mathf.Abs(cellCenter.y - groundY);
                    if (verticalDist > verticalTolerance)
                        continue;

                    // Clean this cell
                    float before = bloodData[arrayX, arrayY];
                    bloodData[arrayX, arrayY] = Mathf.Max(0, bloodData[arrayX, arrayY] - cleanAmount);

                    if (before > 0.001f)
                        cleanedCount++;
                }
            }

            if (cleanedCount > 0)
            {
                needsVisualUpdate = true;
            }
        }

        /// <summary>
        /// Get blood amount at a specific world position
        /// </summary>
        public float GetBloodAtPosition(Vector2 worldPosition)
        {
            Vector3Int cellPos = floorTilemap.WorldToCell(worldPosition);
            int arrayX = cellPos.x - gridOffset.x;
            int arrayY = cellPos.y - gridOffset.y;

            if (arrayX < 0 || arrayX >= gridWidth || arrayY < 0 || arrayY >= gridHeight)
                return 0f;

            return bloodData[arrayX, arrayY];
        }

        /// <summary>
        /// Get blood amount in a radius
        /// </summary>
        public float GetBloodInRadius(Vector2 worldPosition, float radius)
        {
            Vector3Int centerCell = floorTilemap.WorldToCell(worldPosition);
            int radiusCells = Mathf.CeilToInt(radius / floorTilemap.cellSize.x);

            float total = 0f;

            for (int x = -radiusCells; x <= radiusCells; x++)
            {
                for (int y = -radiusCells; y <= radiusCells; y++)
                {
                    Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);

                    int arrayX = cellPos.x - gridOffset.x;
                    int arrayY = cellPos.y - gridOffset.y;

                    if (arrayX < 0 || arrayX >= gridWidth || arrayY < 0 || arrayY >= gridHeight)
                        continue;

                    Vector2 cellCenter = floorTilemap.CellToWorld(cellPos);
                    if (Vector2.Distance(worldPosition, cellCenter) <= radius)
                    {
                        total += bloodData[arrayX, arrayY];
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// Get count of cells with blood
        /// </summary>
        public int GetBloodyCellCount()
        {
            int count = 0;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (bloodData[x, y] > 0.01f)
                        count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Get total blood currently in the level
        /// </summary>
        public float GetCurrentBloodInLevel()
        {
            float total = 0f;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    total += bloodData[x, y];
                }
            }

            return total;
        }

        /// <summary>
        /// Get total blood generated since level start
        /// </summary>
        public float GetTotalBloodGenerated()
        {
            return totalBloodGenerated;
        }

        /// <summary>
        /// Get percentage of blood cleaned (0-100)
        /// </summary>
        public float GetPercentageCleaned()
        {
            if (totalBloodGenerated <= 0)
                return 100f; // No blood generated = 100% clean

            float currentBlood = GetCurrentBloodInLevel();
            float bloodCleaned = totalBloodGenerated - currentBlood;

            return (bloodCleaned / totalBloodGenerated) * 100f;
        }

        /// <summary>
        /// Get percentage of blood remaining (0-100)
        /// </summary>
        public float GetPercentageRemaining()
        {
            return 100f - GetPercentageCleaned();
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying || bloodData == null || grid == null)
                return;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    float bloodAmount = bloodData[x, y];

                    if (bloodAmount > 0.01f)
                    {
                        Vector3Int cellPos = new Vector3Int(x + gridOffset.x, y + gridOffset.y, 0);
                        Vector3 worldPos = floorTilemap.CellToWorld(cellPos);
                        Vector3 cellCenter = worldPos + floorTilemap.cellSize * 0.5f;

                        // Color intensity based on blood amount
                        Color gizmoColor = new Color(1f, 0f, 0f, bloodAmount);
                        Gizmos.color = gizmoColor;

                        // Draw cube at cell position
                        Vector3 cubeSize = new Vector3(floorTilemap.cellSize.x * 0.9f, floorTilemap.cellSize.y * 0.9f, 0.1f);
                        Gizmos.DrawCube(cellCenter, cubeSize);

                        // Draw wire cube outline
                        Gizmos.color = new Color(0.8f, 0f, 0f, 1f);
                        Gizmos.DrawWireCube(cellCenter, cubeSize);
                    }
                }
            }
#if UNITY_EDITOR //Only compile this into editor builds, not prod deployment.
            // Draw stats in corner
            Vector3 statsPos = Camera.main.ViewportToWorldPoint(new Vector3(0.05f, 0.95f, 10f));
            Handles.Label(statsPos,
                $"Blood: {GetCurrentBloodInLevel():F2}\n" +
                $"Generated: {GetTotalBloodGenerated():F2}\n" +
                $"Cleaned: {GetPercentageCleaned():F1}%");
#endif
        }
    }
}