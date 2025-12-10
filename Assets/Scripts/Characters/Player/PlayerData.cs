using Interfaces;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerData : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private Animator animator;
    [SerializeField] private int health = 10;
    [SerializeField] public LayerMask groundLayer;

    public bool isGrounded = true;
    private bool isCleaning;

    private Rigidbody2D playerRigidBody;
    private Collider2D playerCollider;

    private bool isDead;
    
    // Ground-based cleaning cache
    //private float groundY;
    // Debugging
    
    private Vector2[] raycastPositions; // For debug visualization

    // region basic
    private void Start()
    {
        if (BloodSystem.Instance == null)
        {
            Debug.LogError("BloodSystem.Instance is null! Make sure BloodSystem is in the scene.");
        }
        else
        {
            Debug.Log("BloodSystem found and ready.");
        }
    }

    private void Awake()
    {
        playerCollider = GetComponent<Collider2D>();
        playerRigidBody = GetComponent<Rigidbody2D>();
    }
    
    public void SetHealth(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            isDead = true;
            playerCollider.enabled = false;
            return;
        }
    }

    public void SetIsCleaning(bool isCleaning)
    {
        this.isCleaning = isCleaning;
    }

    public bool IsDead() => isDead;
    public bool IsCleaning() => isCleaning;
    //public float GetGroundY() => groundY;    
    public float GetHealthPercent() => health/10f;

    public Rigidbody2D GetRb() => playerRigidBody;

    public Collider2D GetCollider2D() => playerCollider;
    public Animator GetPlayerAnimator() => animator;
  
    public LayerMask GetGroundLayer() => groundLayer;
}