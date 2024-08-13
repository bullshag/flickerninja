using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb2D;
    private Collider2D coll2D; // Reference to the Collider2D component
    void Start()
    {
        coll2D = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
    }

    private bool isGrounded;
    void Update()
    {
        float move = Input.GetAxis("Horizontal");
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(move));

        // Set the "isFalling" parameter based on vertical velocity and grounded status
        if (rb2D.velocity.y < 0 && !isGrounded)
        {
            animator.SetBool("isFalling", true);
        }
        else
        {
            animator.SetBool("isFalling", false);
        }

        // Trigger jump animation
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            animator.SetTrigger("Jump");
        }

        // Flip character sprite based on movement direction
        if (move > 0 && transform.localScale.x < 0)
        {
            Flip();
        }
        else if (move < 0 && transform.localScale.x > 0)
        {
            Flip();
        }
    }

    void Flip()
    {
        Vector3 characterScale = transform.localScale;
        characterScale.x *= -1;
        transform.localScale = characterScale;
    }

    // Collision detection for ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
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

