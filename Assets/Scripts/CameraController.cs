using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float smoothSpeed = 3f; // Glättung
    public Vector3 offset = new Vector3(0, 4f, -10); // Kamera-Offset
    private PlayerData player;
    public float cameraYThreshold;
    
    private float worldXMin = 0;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    public void Awake()
    {
        player = FindFirstObjectByType<PlayerData>();
        if (player == null)
        {
            Debug.LogError("PlayerData fehlt auf " + gameObject.name);
            return;
        }
            
    }
   

    // Update is called once per frame
    void LateUpdate()
    {   
        if(player.IsDead())
        {
            return;
        }

        Vector3 desiredPosition = player.transform.position + offset;
        //camera if player.y > 0 moves 
      
        desiredPosition.y = Mathf.Max(player.transform.position.y-cameraYThreshold, 0);

        desiredPosition.x = Mathf.Max(desiredPosition.x, worldXMin);

        //float cameraYPos = Mathf.Max(player.transform.position.y, cameraYThreshold);
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

   
}
