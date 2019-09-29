using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Handles movement and basic movement animation
 * and lag associated with animation
 */

public class CharacterController2D : MonoBehaviour, ILaggable
{
    Rigidbody2D rb;
    Animator animator;
    public Laggable lag { get; set; }
    //Jump control
    [SerializeField] Transform groundCheck;
    float groundCheckRadius = 0.2f;
    [SerializeField] float jumpVelocity = 12;
    [SerializeField] float jumpSmoothing = 0.2f;
    [SerializeField] float maxJumpTime = 0.2f;
    private float currentJumpTime = 0;
    private Vector2 refJumpVelocity = Vector2.zero;

    //Movement control
    [SerializeField] float speed = 10;
    [SerializeField] float movementSmoothing = 0.05f;
    private Vector2 refMoveVelocity = Vector2.zero;
    bool facingRight = true;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponent<Animator>();
    }
    void Update() {
        animator.speed = lag.TimeScale;
    }
    //Required by interface
    public void LagUpdate() {}
    public void FixedLagUpdate() {}
    //Pass in a value between -1 and 1
    public void Move(float xSpeed) {
        animator.SetFloat("Speed", Mathf.Abs(xSpeed));
        if((xSpeed > 0 && !facingRight) || (xSpeed < 0 && facingRight)) {
            Flip();
        }
        xSpeed *= speed;
        Vector3 desiredVelocity = new Vector2(xSpeed, rb.velocity.y);
        rb.velocity = Vector2.SmoothDamp(rb.velocity, desiredVelocity, ref refMoveVelocity, movementSmoothing);
    }

    //Call consistently, manages falling physics as well
    public void Jump(bool jump) {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, LayerMask.GetMask("Ground"));
        if(grounded)
            currentJumpTime = 0;
        
        if(jump && currentJumpTime < maxJumpTime) {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            currentJumpTime += Time.deltaTime;
        }
        else {
            //Once stop jumping, don't let jump in midair
            currentJumpTime = maxJumpTime;
            Vector2 desiredVelocity = rb.velocity;
            if(!grounded) {
                desiredVelocity.y = -jumpVelocity;
            }
            else {
                return;
            }
            //Make this based on real time?
            rb.velocity = Vector2.SmoothDamp(rb.velocity, desiredVelocity, ref refJumpVelocity, jumpSmoothing * lag.TimeScale);
        }
    }

    void Flip() {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        facingRight = !facingRight;
    }
}
