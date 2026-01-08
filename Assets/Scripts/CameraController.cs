using Characters.Player;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Smooth Settings")]
    public float smoothSpeed = 0.1f;
    
    [Header("Camera Position")]
    public Vector3 offset = new Vector3(0, 5f, -10);
    [Tooltip("Negativ = camera mai sus, Pozitiv = camera mai jos")]
    public float cameraYThreshold = -3f;
    
    [Header("Limits")]
    public float worldXMin = 0;

    private PlayerData playerData;

    public void Awake()
    {
        playerData = FindFirstObjectByType<PlayerData>();
        if (!playerData)
        {
            throw new UnityException("PlayerData access object not found");
        }
    }

    void LateUpdate()
    {   
        if(playerData.IsDead)
        {
            return;
        }

        Vector3 desiredPosition = playerData.Position + offset;
        desiredPosition.y = playerData.Position.y - cameraYThreshold;
        desiredPosition.x = Mathf.Max(desiredPosition.x, worldXMin);
        
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            smoothSpeed * Time.deltaTime * 50f
        );
        
        transform.position = smoothedPosition;
    }
}