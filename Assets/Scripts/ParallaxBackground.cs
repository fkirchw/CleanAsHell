using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cam;
    public float parallaxFactor = 0.3f;   // kleiner = bewegt sich langsamer

    private Vector3 startPos;

    void Start()
    {
        if (cam == null)
            cam = Camera.main.transform;

        // Ausgangsposition merken
        startPos = transform.position;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(
            startPos.x + cam.position.x * parallaxFactor,
            startPos.y + cam.position.y * parallaxFactor,
            transform.position.z
        );
    }
}
