using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class PlayerCleaningSystem: MonoBehaviour
{
    [Header("Cleaning Settings")]
    [SerializeField] private float cleanRadius = 2f;
    [SerializeField] private float cleanRate = 0.5f;

    [Header("Ground-Based Cleaning")]
    private bool useGroundBasedCleaning = true;
    [SerializeField] private float maxCleaningHeight = 2f;
    [SerializeField] private float verticalTolerance = 0.3f;
    [SerializeField] private bool useMultiRaycast = true;
    [SerializeField] private int raycastCount = 5;
    [SerializeField] private float raycastSpread = 1f;

    [Header("Foam Visual Feedback")]
    [SerializeField] private ParticleSystem foamParticles;
    [SerializeField] private float groundCheckDistance = 5f;
    [SerializeField] private int foamParticlesPerSecond = 20;

    // -----------------------------
    // NON-serialized internal fields
    // -----------------------------

    private float foamParticleTimer;
    private int cleaningFrames;
    private Vector2[] raycastPositions;
    private float groundY;
    private float bloodCleanedThisFrame;

    private PlayerData player;
    private Animator animator;

    public void Start()
    {
        if (PlayerData.Instance != null)
        {
            player = PlayerData.Instance;
        } else
        {
            Debug.Log("Instanz nicht gesetzt");
            return;
        }
        animator = player.GetPlayerAnimator();
    }

    public void Update()
    {
        bool fire1Pressed = Input.GetButtonDown("Fire1");
        bool fire2Down = Input.GetButton("Fire2");
        bool isHitting = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");

        if (fire2Down && !isHitting)
        {
            if (!player.IsCleaning())
            {
                StartCleaning();
            }

            PerformCleaning();
            SpawnGroundFoam();
        }
        else if (player.IsCleaning())
        {
            StopCleaning();
        }
    }

    void StartCleaning()
    {
        player.SetIsCleaning(true);
        foamParticleTimer = 0f;
        cleaningFrames = 0;
        player.hasGroundHeight = false;

        if (useGroundBasedCleaning)
        {
            FindGroundHeight();
        }

        Debug.Log($"Started cleaning! Ground-based: {useGroundBasedCleaning}, Ground Y: {groundY:F2}");
    }

    void StopCleaning()
    {
        player.SetIsCleaning(false);
        player.hasGroundHeight = false;
        raycastPositions = null;
        Debug.Log($"Stopped cleaning. Cleaned for {cleaningFrames} frames.");
    }

    void FindGroundHeight()
    {
        if (useMultiRaycast)
        {
            FindGroundHeightMultiRaycast();
        }
        else
        {
            FindGroundHeightSingleRaycast();
        }
    }

    void FindGroundHeightSingleRaycast()
    {
        // Original single raycast method
        RaycastHit2D hit = Physics2D.Raycast(
            player.transform.position,
            Vector2.down,
            maxCleaningHeight,
            player.groundLayer
        );

        if (hit.collider != null)
        {
            groundY = hit.point.y;
            player.hasGroundHeight = true;
        }
        else
        {
            groundY = player.transform.position.y;
            player.hasGroundHeight = true;
            Debug.LogWarning($"No ground found below player, using player Y={groundY:F2}");
        }
    }

    void FindGroundHeightMultiRaycast()
    {
        // Cast multiple rays in a line to catch platform edges
        // This helps when player is at edge of platform with pit below

        raycastPositions = new Vector2[raycastCount];
        float closestGroundY = float.NegativeInfinity;
        bool foundAnyGround = false;

        // Calculate ray positions spread out horizontally
        float startX = -raycastSpread / 2f;
        float stepX = raycastCount > 1 ? raycastSpread / (raycastCount - 1) : 0;

        for (int i = 0; i < raycastCount; i++)
        {
            float offsetX = startX + (stepX * i);
            Vector2 rayOrigin = new Vector2(player.transform.position.x + offsetX, player.transform.position.y);
            raycastPositions[i] = rayOrigin;

            RaycastHit2D hit = Physics2D.Raycast(
                rayOrigin,
                Vector2.down,
                maxCleaningHeight,
                player.groundLayer
            );

            if (hit.collider != null)
            {
                // Found ground - use the closest (highest) ground
                if (hit.point.y > closestGroundY)
                {
                    closestGroundY = hit.point.y;
                    foundAnyGround = true;
                }
            }
        }

        if (foundAnyGround)
        {
            groundY = closestGroundY;
            player.hasGroundHeight = true;
            Debug.Log($"Multi-raycast found ground at Y={groundY:F2}");
        }
        else
        {
            // No ground found by any raycast - fallback to player height
            groundY = player.transform.position.y;
            player.hasGroundHeight = true;
            Debug.LogWarning($"Multi-raycast found no ground, using player Y={groundY:F2}");
        }
    }

    void PerformCleaning()
    {
        if (BloodSystem.Instance == null)
        {
            Debug.LogWarning("Cannot clean - BloodSystem.Instance is null!");
            return;
        }

        // Update ground height occasionally in case player moved
        if (useGroundBasedCleaning && cleaningFrames % 10 == 0)
        {
            FindGroundHeight();
        }

        float bloodBefore = BloodSystem.Instance.GetBloodInRadius(player.transform.position, cleanRadius);

        float cleanAmount = cleanRate * Time.deltaTime;

        if (useGroundBasedCleaning && player.hasGroundHeight)
        {
            BloodSystem.Instance.CleanBloodOnGround(
                player.transform.position,
                groundY,
                verticalTolerance,
                cleanRadius,
                cleanAmount
            );
        }
        else
        {
            BloodSystem.Instance.CleanBlood(
                player.transform.position,
                cleanRadius,
                cleanAmount
            );
        }

        float bloodAfter = BloodSystem.Instance.GetBloodInRadius(player.transform.position, cleanRadius);
        bloodCleanedThisFrame = bloodBefore - bloodAfter;
        cleaningFrames++;

        if (cleaningFrames % 30 == 0)
        {
            Debug.Log(
                $"Cleaning: {bloodBefore:F3} -> {bloodAfter:F3} (cleaned {bloodCleanedThisFrame:F4}, rate: {cleanAmount:F4})");
        }
    }

    void SpawnGroundFoam()
    {
        if (!foamParticles)
        {
            return;
        }

        foamParticleTimer += Time.deltaTime;
        float interval = 1f / foamParticlesPerSecond;

        while (foamParticleTimer >= interval)
        {
            foamParticleTimer -= interval;
            EmitFoamParticle();
        }
    }

    //endsubregion
    //subregion cosmetic

    void EmitFoamParticle()
    {
        Vector2 randomOffset = Random.insideUnitCircle * cleanRadius;
        Vector2 spawnPosition = (Vector2)player.transform.position + randomOffset;

        float targetY = useGroundBasedCleaning && player.hasGroundHeight ? groundY : player.transform.position.y;

        RaycastHit2D hit = Physics2D.Raycast(
            new Vector2(spawnPosition.x, targetY + groundCheckDistance),
            Vector2.down,
            groundCheckDistance * 2f,
            player.groundLayer
        );

        if (hit.collider)
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = hit.point + Vector2.up * 0.1f;

            Vector2 horizontalVel = Random.insideUnitCircle * 0.5f;
            emitParams.velocity = new Vector3(horizontalVel.x, 0, 0);

            emitParams.startSize = Random.Range(0.2f, 0.5f);
            emitParams.rotation = Random.Range(0f, 360f);
            emitParams.startLifetime = Random.Range(1.5f, 2.5f);

            foamParticles.Emit(emitParams, 1);
        }
    }


    public void DebugCleaning()
    {   
        if(player == null) return;

        // Draw cleaning radius
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(player.transform.position, cleanRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(player.transform.position, cleanRadius);

        // Draw ground-based cleaning visualization
        if (Application.isPlaying && useGroundBasedCleaning && player.hasGroundHeight)
        {
            // Draw the ground line
            Gizmos.color = Color.green;
            Vector2 left = new Vector2(player.transform.position.x - cleanRadius, groundY);
            Vector2 right = new Vector2(player.transform.position.x + cleanRadius, groundY);
            Gizmos.DrawLine(left, right);

            // Draw vertical tolerance
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Vector3 toleranceSize = new Vector3(cleanRadius * 2, verticalTolerance * 2, 0);
            Gizmos.DrawCube(new Vector3(player.transform.position.x, groundY, 0), toleranceSize);

            // Draw multi-raycast positions
            if (useMultiRaycast && raycastPositions != null)
            {
                for (int i = 0; i < raycastPositions.Length; i++)
                {
                    Vector2 rayStart = raycastPositions[i];
                    Vector2 rayEnd = new Vector2(rayStart.x, groundY);

                    // Color based on whether this ray hit ground
                    bool hitGround = Mathf.Abs(rayEnd.y - groundY) < 0.1f;
                    Gizmos.color = hitGround ? Color.yellow : Color.red;

                    Gizmos.DrawLine(rayStart, rayEnd);
                    Gizmos.DrawWireSphere(rayStart, 0.1f);
                }
            }
            else
            {
                // Draw single raycast
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(player.transform.position, new Vector2(player.transform.position.x, groundY));
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        DebugCleaning();
    }
}
