using Characters.Player;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float smoothTime = 0.2f;
    public float cameraHeightAbovePlayer = 3f;
    public float worldXMin = 0;

    private PlayerData playerData;
    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
        playerData = FindFirstObjectByType<PlayerData>();
        if (!playerData)
            throw new UnityException("PlayerData access object not found");

        offset = transform.position - playerData.Position;
        offset.y = cameraHeightAbovePlayer;     // ensure vertical spacing is what you want
    }

    void FixedUpdate()
    {
        if (playerData.IsDead) return;

        Vector3 target = playerData.Position + offset;

        // clamp X AFTER offset
        target.x = Mathf.Max(target.x, worldXMin);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            target,
            ref velocity,
            smoothTime
        );
    }
}