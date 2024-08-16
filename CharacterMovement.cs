using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed of the character
    public float jumpForce = 10f; // Force applied for jumping
    public float dodgeDistance = 5f; // Distance for dodging
    public LayerMask groundLayer; // LayerMask for ground detection
    public LayerMask wallLayer; // LayerMask for wall detection
    public float dodgeInertiaMultiplier = 1f; // Multiplier to adjust the horizontal inertia during dodge
    public GameObject dodgeRing;
    private Animator animator;
    private Rigidbody2D rb2D;
    public bool isGrounded;
    public bool canDodge = true; // To check if the player can dodge
    public float rightStickHorizontal;
    public float rightStickVertical;
    public int blinkStage;
    public GameObject smokeCloud;
    public playerData playerData;
    public GameObject swordHitBox;
    void Start()
    {
        playerData = GetComponent<playerData>();
        animator = GetComponent<Animator>();
        blinkStage = 0;
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
        {
            Debug.LogError("Rigidbody2D component is missing.");
        }
    }

    void Update()
    {
        rightStickHorizontal = Input.GetAxis("DodgeHorizontal");
        rightStickVertical = Input.GetAxis("DodgeVertical");
        groundAttack();
        MoveCharacter();
        Jump();

        if ((rightStickHorizontal != 0f || rightStickVertical != 0f) && !isGrounded && canDodge && Input.GetButton("DodgeRadius") && (playerData.stCurrent - 25) >= 0)
        {
            playerData.stCurrent -= 25;
            animator.Play("blinkBegin");
            GameObject tempSmokeCloud = smokeCloud;
            Instantiate(tempSmokeCloud, gameObject.transform.position, Quaternion.identity);
            Dodge();
            Destroy(tempSmokeCloud, 1f);
            Debug.Log("Dodged!");


        }

        if (Input.GetButton("DodgeRadius") && canDodge)
        {
            dodgeRing.SetActive(true);

        }
        else
        {
            dodgeRing.SetActive(false);
        }
    }

    private void groundAttack()
    {

        switch (isGrounded)
        {
            case true:
                if (Input.GetButtonDown("attack") && playerData.stCurrent - 15 >= 0)
                {
                    StartCoroutine(toggleSwordHitbox());
                    playerData.stCurrent -= 15;
                    if (rb2D.velocity.x == 0f)
                    {
                        animator.Play("slashAnim");

                    }
                    else
                    {

                        animator.Play("jumpSlashAnim");
                    }
                }
                break;
            case false:
                if (Input.GetButtonDown("attack") && playerData.stCurrent - 15 >= 0)
                {
                    StartCoroutine(toggleSwordHitbox());
                    playerData.stCurrent -= 15;
                    animator.Play("jumpSlashAnim");
                }
                break;
        }

    }

    IEnumerator toggleSwordHitbox()
    {
        // Enable the GameObject
        swordHitBox.SetActive(true);

        // Wait for the specified duration
        yield return new WaitForSeconds(1f);

        // Disable the GameObject
        swordHitBox.SetActive(false);
    }
    void MoveCharacter()
    {
        float moveInput = Input.GetAxis("Horizontal");
        if (isGrounded && moveInput != 0f)
        {
            rb2D.velocity = new Vector2(moveInput * moveSpeed, rb2D.velocity.y);

        }
    }
        void Jump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                float moveInput = Input.GetAxis("Horizontal");
                rb2D.velocity = new Vector2(moveInput * moveSpeed, jumpForce);
            }
        }








        void Dodge()
        {
            // Fetch the dodge direction using raw input values
            float dodgeHorizontal = Input.GetAxisRaw("DodgeHorizontal");
            float dodgeVertical = Input.GetAxisRaw("DodgeVertical");

            Vector2 dodgeDirection = new Vector2(rightStickHorizontal, rightStickVertical).normalized;

            // Ensure that the dodge direction is valid
            if (dodgeDirection != Vector2.zero)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dodgeDirection, dodgeDistance, wallLayer);

                if (hit.collider != null)
                {
                    transform.position = hit.point;
                }
                else
                {
                    transform.position += (Vector3)dodgeDirection * dodgeDistance;
                }

                // Flip the character to face the dodge direction
                if (dodgeDirection.x != 0)
                {
                    // Flip the character horizontally based on dodge direction
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Sign(dodgeDirection.x) * Mathf.Abs(scale.x);
                    transform.localScale = scale;
                }

                rb2D.velocity = new Vector2(dodgeDirection.x * moveSpeed * dodgeInertiaMultiplier, rb2D.velocity.y);

                canDodge = false;
                animator.SetBool("CanDodge", canDodge);
            }
        }


        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
                canDodge = true; // Allow dodging again when grounded
                blinkStage = 0;

                animator.SetBool("CanDodge", canDodge);
                animator.SetInteger("blinkStage", blinkStage);
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = false;
            }
        }
    }


