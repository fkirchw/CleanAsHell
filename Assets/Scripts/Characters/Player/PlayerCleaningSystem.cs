using Blood;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerCleaningSystem : MonoBehaviour
    {
        [Header("Cleaning Settings")] [SerializeField]
        private float cleanRadius = 2f;

        [SerializeField] private float cleanRate = 0.5f;

        [Header("Ground-Based Cleaning")] [SerializeField]
        private bool useGroundBasedCleaning = true;
        [SerializeField] private float maxCleaningHeight = 2f;
        [SerializeField] private float verticalTolerance = 0.3f;
        [SerializeField] private bool useMultiRaycast = true;
        [SerializeField] private int raycastCount = 5;
        [SerializeField] private float raycastSpread = 1f;

        [Header("Foam Visual Feedback")] [SerializeField]
        private ParticleSystem foamParticles;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 5f;
        [SerializeField] private int foamParticlesPerSecond = 20;

        private PlayerMovement movement;
        private Animator animator;
        private bool isCleaning;
        private float groundY;
        private bool hasGroundHeight;
        private float foamParticleTimer;
        private int cleaningFrames;
        private Vector2[] raycastPositions;

        public bool IsCleaning => isCleaning;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            animator = GetComponent<Animator>();
            
        }

        private void Update()
        {
            bool fire2Down = Input.GetButton("Fire2");
            bool isInHitAnimation = animator.GetCurrentAnimatorStateInfo(0).IsName("Hit");

            if (fire2Down && !isInHitAnimation)
            {
                if (!isCleaning)
                    StartCleaning();

                PerformCleaning();
                SpawnGroundFoam();
            }
            else if (isCleaning)
            {
                StopCleaning();
            }
        }

        private void StartCleaning()
        {
            isCleaning = true;
            foamParticleTimer = 0f;
            cleaningFrames = 0;
            hasGroundHeight = false;

            if (useGroundBasedCleaning)
                FindGroundHeight();
        }

        private void StopCleaning()
        {
            isCleaning = false;
            hasGroundHeight = false;
            raycastPositions = null;
        }

        private void FindGroundHeight()
        {
            if (useMultiRaycast)
                FindGroundHeightMultiRaycast();
            else
                FindGroundHeightSingleRaycast();
        }

        private void FindGroundHeightSingleRaycast()
        {
            float? ground = movement.GetGroundHeightBelow(maxCleaningHeight, groundLayer);
            if (ground.HasValue)
            {
                groundY = ground.Value;
                hasGroundHeight = true;
            }
            else
            {
                groundY = transform.position.y;
                hasGroundHeight = true;
            }
        }

        private void FindGroundHeightMultiRaycast()
        {
            raycastPositions = new Vector2[raycastCount];
            float closestGroundY = float.NegativeInfinity;
            bool foundAnyGround = false;

            float startX = -raycastSpread / 2f;
            float stepX = raycastCount > 1 ? raycastSpread / (raycastCount - 1) : 0;

            for (int i = 0; i < raycastCount; i++)
            {
                float offsetX = startX + (stepX * i);
                Vector2 rayOrigin = new Vector2(transform.position.x + offsetX, transform.position.y);
                raycastPositions[i] = rayOrigin;

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, maxCleaningHeight, groundLayer);

                if (hit.collider != null && hit.point.y > closestGroundY)
                {
                    closestGroundY = hit.point.y;
                    foundAnyGround = true;
                }
            }

            groundY = foundAnyGround ? closestGroundY : transform.position.y;
            hasGroundHeight = true;
        }

        private void PerformCleaning()
        {
            if (BloodSystem.Instance == null) return;

            if (useGroundBasedCleaning && cleaningFrames % 10 == 0)
                FindGroundHeight();

            float cleanAmount = cleanRate * Time.deltaTime;

            if (useGroundBasedCleaning && hasGroundHeight)
            {
                BloodSystem.Instance.CleanBloodOnGround(
                    transform.position, groundY, verticalTolerance, cleanRadius, cleanAmount);
            }
            else
            {
                BloodSystem.Instance.CleanBlood(transform.position, cleanRadius, cleanAmount);
            }

            cleaningFrames++;
        }

        private void SpawnGroundFoam()
        {
            if (!foamParticles) return;

            foamParticleTimer += Time.deltaTime;
            float interval = 1f / foamParticlesPerSecond;

            while (foamParticleTimer >= interval)
            {
                foamParticleTimer -= interval;
                EmitFoamParticle();
            }
        }

        private void EmitFoamParticle()
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * cleanRadius;
            Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

            float targetY = useGroundBasedCleaning && hasGroundHeight ? groundY : transform.position.y;

            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(spawnPosition.x, targetY + groundCheckDistance),
                Vector2.down,
                groundCheckDistance * 2f,
                groundLayer);

            if (hit.collider)
            {
                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
                {
                    position = hit.point + Vector2.up * 0.1f,
                    velocity = new Vector3(UnityEngine.Random.insideUnitCircle.x * 0.5f, 0, 0),
                    startSize = UnityEngine.Random.Range(0.2f, 0.5f),
                    rotation = UnityEngine.Random.Range(0f, 360f),
                    startLifetime = UnityEngine.Random.Range(1.5f, 2.5f)
                };
                foamParticles.Emit(emitParams, 1);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawSphere(transform.position, cleanRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, cleanRadius);

            if (Application.isPlaying && useGroundBasedCleaning && hasGroundHeight)
            {
                Gizmos.color = Color.green;
                Vector2 left = new Vector2(transform.position.x - cleanRadius, groundY);
                Vector2 right = new Vector2(transform.position.x + cleanRadius, groundY);
                Gizmos.DrawLine(left, right);

                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Vector3 toleranceSize = new Vector3(cleanRadius * 2, verticalTolerance * 2, 0);
                Gizmos.DrawCube(new Vector3(transform.position.x, groundY, 0), toleranceSize);
            }
        }
    }
}