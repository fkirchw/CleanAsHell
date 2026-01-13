using Characters.Player;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Smooth Settings")]
    public float smoothSpeed = 0.125f; // Valoare mai standard
    
    [Header("Camera Follow")]
    [Tooltip("Camera va fi cu atât mai sus față de player (valoare pozitivă)")]
    public float cameraHeightAbovePlayer = 3f;
    
    [Header("Limits")]
    public float worldXMin = 0;

    private PlayerData playerData;
    private Vector3 initialOffset; // Offset-ul inițial față de player

    public void Awake()
    {
        playerData = FindFirstObjectByType<PlayerData>();
        if (!playerData)
        {
            throw new UnityException("PlayerData access object not found");
        }
        
        // Calculează offset-ul inițial față de player
        // Aceasta va fi distanța CONSTANTĂ dintre camera și player
        initialOffset = transform.position - playerData.Position;
        
        Debug.Log($"Camera initial offset: {initialOffset}");
    }

    void LateUpdate()
    {   
        if (playerData.IsDead) return;

        // 1. Obține poziția curentă a player-ului
        Vector3 playerPos = playerData.Position;
        
        // 2. Calculează poziția țintă a camerei
        // Camera va urma player-ul, păstrând offset-ul vertical constant
        Vector3 targetPosition = new Vector3(
            playerPos.x,
            playerPos.y + cameraHeightAbovePlayer, // Camera mereu deasupra player-ului
            transform.position.z // Menține distanța Z
        );
        
        // 3. Aplică limitarea pe X
        targetPosition.x = Mathf.Max(targetPosition.x, worldXMin);
        
        // 4. Interpolare lină către poziția țintă
        // Folosește SmoothDamp pentru mișcare mai naturală
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime * 60f // 60f pentru frame-rate independent
        );
    }
    
    // Metodă pentru debugging
    void OnDrawGizmosSelected()
    {
        if (playerData != null && Application.isPlaying)
        {
            // Arată poziția țintă a camerei
            Gizmos.color = Color.yellow;
            Vector3 targetPos = new Vector3(
                Mathf.Max(playerData.Position.x, worldXMin),
                playerData.Position.y + cameraHeightAbovePlayer,
                transform.position.z
            );
            Gizmos.DrawWireCube(targetPos, Vector3.one * 0.5f);
            
            // Linie între camera și player
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, playerData.Position);
        }
    }
}