using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BatScript : MonoBehaviour
{
    public GameObject playerObject;
    public playerData playerData;
    public float aggroSpeed = 2f;
    public float speed = 1.5f;
    public float detectionInterval = 1f;
    public float wallAvoidanceDistance = 2f;
    public LayerMask obstacleMask; // Layer for obstacles
    public LayerMask playerMask;   // Layer for player

    private Animator batanimator;
    private Transform player;
    public bool chasingPlayer = false;
    private float flipChance = 0.01f;
    private float timeSinceLastFlip = 0f;

    private Collider2D bodyCollider;  // Bat's body collider
    private Collider2D aggroCollider;  // Aggro range collider
    private Collider2D sightCollider;  // Sight cone collider
    private Rigidbody2D rb;

    public int hp;

    private bool isFacingRight = true;
    public float bounceDrag = 2f;   // How quickly the bounce slows down
    private bool isBouncing;
    public float bounceForce = 10f; // Force applied when bouncing away
    public float damageCooldown = 0.5f; // Cooldown time in seconds between damages
    private float lastDamageTime = 0f; // Time when the bat was last damaged

    private bool isInWeaponHitbox = false; // Track if the bat is inside the weapon hitbox

    void Start()
    {
        hp = 20;
        playerData = playerObject.GetComponent<playerData>();
        batanimator = GetComponent<Animator>();
        isBouncing = false;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;  // Disable gravity for flying

        bodyCollider = GetComponent<Collider2D>(); // Assuming the body collider is the main collider
        aggroCollider = transform.Find("aggroRange").GetComponent<Collider2D>();
        sightCollider = transform.Find("sightRange").GetComponent<Collider2D>();

        InvokeRepeating("DetectAndMove", 0, detectionInterval);
    }

    public float minDistanceToPlayer = 2f; // Set this to your desired distance

    void Update()
    {
        if (chasingPlayer && player != null)
        {
            // Check the distance between the bat and the player
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // If the bat is within the specified distance, play the attack animation
            if (distanceToPlayer <= minDistanceToPlayer)
            {
                batanimator.Play("batAttackMelee");
            }

            // Apply bounce drag if needed
            if (isBouncing)
            {
                ApplyBounceDrag();
            }
        }

        // Apply continuous damage if in weapon hitbox
        if (isInWeaponHitbox && Input.GetButtonDown("attack"))
        {
            ApplyDamageOverTime();
        }
    }

    void DetectAndMove()
    {
        if (!isBouncing)
        {
            if (chasingPlayer)
            {
                if (player != null)
                {
                    if (IsClearPath(player.position))
                    {
                        MoveTowards(player.position);
                    }
                    else
                    {
                        // Stop chasing if the path to the player is blocked
                        chasingPlayer = false;
                    }
                }
            }
            else
            {
                RandomMovement();
                IncreaseFlipChance();
            }
        }
    }

    void RandomMovement()
    {
        Vector2 randomPoint = GetWeightedRandomPointInSight();

        if (IsClearPath(randomPoint))
        {
            MoveTowards(randomPoint);
        }

        timeSinceLastFlip += detectionInterval;
        if (UnityEngine.Random.value < flipChance)
        {
            FlipDirection();
            timeSinceLastFlip = 0f;
        }
    }

    void MoveTowards(Vector2 target)
    {
        if (!isBouncing)
        {
            if (chasingPlayer)
            {
                FlipToFacePlayer(); // Flip the bat to face the player
                speed = 3.25f;
            }
            else
            {
                speed = 1.5f;
            }
            Vector2 direction = (target - (Vector2)transform.position).normalized;
            rb.velocity = direction * speed;
        }
    }

    void FlipToFacePlayer()
    {
        if (player != null)
        {
            if (player.position.x > transform.position.x && !isFacingRight)
            {
                Flip();
            }
            else if (player.position.x < transform.position.x && isFacingRight)
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    Vector2 GetWeightedRandomPointInSight()
    {
        Vector2 point;
        PolygonCollider2D polygonCollider = sightCollider as PolygonCollider2D;
        Bounds bounds = polygonCollider.bounds;

        int attempts = 10;
        while (attempts > 0)
        {
            point = new Vector2(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y)
            );

            if (polygonCollider.OverlapPoint(point) && IsAwayFromWall(point))
            {
                return point;
            }

            attempts--;
        }

        do
        {
            point = new Vector2(
                UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                UnityEngine.Random.Range(bounds.min.y, bounds.max.y)
            );
        } while (!polygonCollider.OverlapPoint(point));

        return point;
    }

    bool IsClearPath(Vector2 point)
    {
        RaycastHit2D hit = Physics2D.Linecast(transform.position, point, obstacleMask);
        return hit.collider == null;
    }

    bool IsAwayFromWall(Vector2 point)
    {
        RaycastHit2D hit = Physics2D.Raycast(point, Vector2.right, wallAvoidanceDistance, obstacleMask);
        if (hit.collider != null) return false;

        hit = Physics2D.Raycast(point, Vector2.left, wallAvoidanceDistance, obstacleMask);
        if (hit.collider != null) return false;

        hit = Physics2D.Raycast(point, Vector2.up, wallAvoidanceDistance, obstacleMask);
        if (hit.collider != null) return false;

        hit = Physics2D.Raycast(point, Vector2.down, wallAvoidanceDistance, obstacleMask);
        if (hit.collider != null) return false;

        return true;
    }

    void FlipDirection()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
        flipChance = 0.001f; // Reset flip chance after flipping
    }

    void IncreaseFlipChance()
    {
        flipChance = Mathf.Min(flipChance + 0.001f, 1f);
    }

    void ApplyDamageOverTime()
    {
        // Check if enough time has passed since the last damage
        if (Time.time - lastDamageTime >= damageCooldown)
        {
            // Update last damage time
            lastDamageTime = Time.time;
            Debug.Log("Applying damage over time...");
            Vector2 bounceDirection = (transform.position - player.position).normalized;
            rb.velocity = Vector2.zero; // Reset velocity before bounce
            isBouncing = true; // Start applying bounce drag
            rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Force);
            batanimator.Play("batHitByWeapon");

            // Deduct health from the bat
            hp -= 8; // Adjust this value based on the damage your weapon deals
            if (hp <= 0)
            {
                Destroy(gameObject); // Destroy the bat if health is zero or less
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == bodyCollider) // Ensure only the body collider interacts
        {
            if (other.CompareTag("weaponHitbox"))
            {
                Debug.Log("Weapon hitbox entered...");
                isInWeaponHitbox = true; // Track that the bat is inside the weapon hitbox
            }
        }
        else if (other.CompareTag("Player"))
        {
            player = other.transform;
            chasingPlayer = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == bodyCollider) // Ensure only the body collider interacts
        {
            if (other.CompareTag("weaponHitbox"))
            {
                Debug.Log("Weapon hitbox exited...");
                isInWeaponHitbox = false; // Track that the bat is no longer inside the weapon hitbox
            }
        }
        else if (other.CompareTag("Player"))
        {
            // Check if the collider that left was the player, if so stop chasing
            if (player != null && other.transform == player)
            {
                player = null;
                chasingPlayer = false;
            }
        }
    }

    void ApplyBounceDrag()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            rb.velocity *= (1f - bounceDrag * Time.deltaTime);
        }
        else
        {
            isBouncing = false; // Stop bouncing when velocity is low
        }
    }
}
