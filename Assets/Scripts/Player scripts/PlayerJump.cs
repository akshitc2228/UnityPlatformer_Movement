using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float maxJumpHeight = 2.5f;
    [SerializeField] private float timeToApex = 0.22f;
    [SerializeField] private float jumpTimeMax = 0.15f;

    [Header("Gravity Modifiers")]
    [SerializeField] private float fallMultiplier = 0.75f;
    [SerializeField] private float lowJumpMultiplier = 0.5f;
    [SerializeField] private float gravityReducer = 0.55f;

    [Header("Air Speed Controls")]
    [SerializeField] private float airSpeedModifier = 1.5f;
    [SerializeField] float airAcceleration = 30f;
    [SerializeField] private float maxFallSpeed = 15f;

    [Header("Jump Timers")]
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float airTime = 0.7f;

    [SerializeField] private PhysicsProfile physics;

    private float gravity;
    private float jumpVelocity;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private float airTimer;
    private float jumpTimeCounter;
    private bool isJumping;
    //TODO: consider removing
    //private float _airHorzVelRef = 1.5f;

    private PlayerMovement movement;
    private PlayerWallInteraction wall;

    //double jump
    private bool hasDoubleJumped;

    private bool _isGrounded;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        wall = GetComponent<PlayerWallInteraction>();

        gravity = -2 * maxJumpHeight / Mathf.Pow(timeToApex, 2);
        jumpVelocity = 2 * maxJumpHeight / timeToApex;
    }

    public void HandleJumpInput(bool jumpPressed, bool jumpHeld)
    {
        if (_isGrounded)
        {
            hasDoubleJumped = false;
            coyoteTimer = coyoteTime;
            airTimer = airTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpHeld)
        {
            jumpBufferTimer = jumpBufferTime;
            airTimer = airTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (jumpHeld && jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            isJumping = true;
            hasDoubleJumped = false;
            jumpTimeCounter = jumpTimeMax;
        }

        if(jumpHeld && isJumping)
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            if (jumpTimeCounter > 0)
            {
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if(jumpPressed && !isJumping && !hasDoubleJumped && !wall.TouchingWall)
        {
            isJumping = true;
            jumpTimeCounter = jumpTimeMax;
            hasDoubleJumped = true;
            return;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {   
            isJumping = false;
        }
    }

    public void ApplyJumpPhysics(Rigidbody2D rb, float inputX)
    {
        if (movement.IsDashing)
        {
            isJumping = false;
            return;
        }

        // Jump sustain
        float horizontalVelocity = Mathf.Clamp(rb.velocity.x, airSpeedModifier * inputX, rb.velocity.x);
        if (isJumping && jumpTimeCounter > 0f && rb.velocity.y <= jumpVelocity) {
            rb.velocity = new Vector2(horizontalVelocity, jumpVelocity);
        }

        // Short hop
        if (isJumping && Input.GetKeyUp(KeyCode.Space))
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity * lowJumpMultiplier);

        if (!_isGrounded)
        {
            if (Mathf.Abs(rb.velocity.y) < 2f && airTimer > 0f)
            {
                rb.gravityScale = (gravity / Physics2D.gravity.y) * gravityReducer;

                if (Mathf.Abs(inputX) > 0.01f)
                {
                    float targetX = airSpeedModifier * inputX;
                    float currentX = rb.velocity.x;

                    if(wall.TouchingWall || wall.IsWallJumping)
                    {
                        //whatever thrust is given externally either that or dont fall below a threshold
                        currentX = Mathf.Clamp(currentX, targetX*1.5f, float.MaxValue);
                    }
                    if (Mathf.Sign(currentX) != Mathf.Sign(inputX))
                    {
                        //TODO: put this into a const
                        currentX = targetX * 1.5f;
                    }
                    else
                    {
                        currentX = Mathf.MoveTowards(currentX, targetX, airAcceleration * Time.fixedDeltaTime);
                    }

                    // Apply acceleration toward target
                    float newX = Mathf.MoveTowards(currentX, targetX, airAcceleration * Time.fixedDeltaTime);
                    rb.velocity = new Vector2(newX, rb.velocity.y);
                }

                airTimer = Mathf.Max(0f, airTimer - Time.fixedDeltaTime);
            }
            else if (rb.velocity.y < 0 && airTimer <= 0f)
            {
                rb.gravityScale = (gravity / Physics2D.gravity.y) * fallMultiplier;
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
            }
            else
            {
                rb.gravityScale = gravity / Physics2D.gravity.y;
            }
        }
        else
        {
            rb.gravityScale = gravity / Physics2D.gravity.y;
        }

        physics.GlobalGravityScaleReference = rb.gravityScale;
    }

    public void SetIsGrounded(bool grounded)
    {
        _isGrounded = grounded;
    }
}
