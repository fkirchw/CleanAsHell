using Characters.Player;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float smoothTimeMin = 0.15f;
    public float smoothTimeMax = 0.001f;
    public float cameraHeightAbovePlayer = 2.5f;
    public float worldXMin = 0;
    public float velocityThreshold = 30f;
    public float lookDownOffset = 2f;
    public float lookDownTransitionTime = 0.2f;

    private PlayerData playerData;
    private Rigidbody2D playerRigidbody;
    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;
    private bool wasLookingDown;
    private float lookDownTransitionTimer;

    void Awake()
    {
        playerData = FindFirstObjectByType<PlayerData>();
        if (!playerData)
            throw new UnityException("PlayerData access object not found");

        playerRigidbody = playerData.GetComponent<Rigidbody2D>();
        if (!playerRigidbody)
            throw new UnityException("Rigidbody2D not found on PlayerData object");

        offset = transform.position - playerData.Position;
        offset.y = cameraHeightAbovePlayer;
    }

    void FixedUpdate()
    {
        if (playerData.IsDead) return;

        Vector3 target = playerData.Position + offset;
        target.x = Mathf.Max(target.x, worldXMin);

        if (playerData.IsLookingDown)
        {
            target.y -= lookDownOffset;
        }

        float playerCurrentSpeed = playerRigidbody.linearVelocity.magnitude;
        float currentSmoothTime = CalculateDynamicSmoothTime(playerCurrentSpeed);

        if (playerData.IsLookingDown && playerCurrentSpeed > 10f && Mathf.Abs(playerRigidbody.linearVelocity.y) < 20f)
        {
            currentSmoothTime = Mathf.Max(currentSmoothTime, smoothTimeMin * 0.7f);
        }

        if (wasLookingDown != playerData.IsLookingDown)
        {
            lookDownTransitionTimer = lookDownTransitionTime;
            wasLookingDown = playerData.IsLookingDown;
        }

        if (lookDownTransitionTimer > 0)
        {
            currentSmoothTime = Mathf.Max(currentSmoothTime, smoothTimeMin);
            lookDownTransitionTimer -= Time.fixedDeltaTime;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target,
            ref velocity,
            currentSmoothTime
        );
    }

    private float CalculateDynamicSmoothTime(float playerSpeed)
    {
        if (playerSpeed >= velocityThreshold)
        {
            return smoothTimeMax;
        }
        else
        {
            float speedRatio = playerSpeed / velocityThreshold;
            return Mathf.Lerp(smoothTimeMin, smoothTimeMax, speedRatio);
        }
    }
}