using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float maxJumpHeight = 2.5f;
    [SerializeField] private float timeToApex = 0.22f;
    [SerializeField] private float jumpTimeMax = 0.15f;
    [SerializeField] private float fallMultiplier = 0.75f;
    [SerializeField] private float lowJumpMultiplier = 0.5f;
    [SerializeField] private float gravityReducer = 0.55f;
    [SerializeField] private float airSpeedModifier = 1.5f;
    [SerializeField] private float maxFallSpeed = 15f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float airTime = 0.7f;
    [SerializeField] private bool enableDoubleJump = true;

    private float gravity;
    private float jumpVelocity;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private float airTimer;
    private float jumpTimeCounter;
    private bool isJumping;
    private float _airHorzVelRef = 1.5f;

    private PlayerMovement movement;

    //double jump
    private bool hasDoubleJumped;

    private bool _isGrounded;

    public float CurrentGravityScale { get; private set; }

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();

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

        //was jumpPressed; revert if issues are found
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

        if(jumpPressed && !isJumping && enableDoubleJump && !hasDoubleJumped)
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
        if(movement.IsDashing)
        {
            isJumping = false;
            return;
        }
        if (isJumping && jumpTimeCounter > 0f && rb.velocity.y <= jumpVelocity)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }

        if (isJumping && Input.GetKeyUp(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity * lowJumpMultiplier);
        }

        if (!_isGrounded)
        {
            if(Mathf.Abs(rb.velocity.y) < 2f && airTimer > 0f)
            {
                rb.gravityScale = (gravity / Physics2D.gravity.y) * gravityReducer;

                float targetX = rb.velocity.x * airSpeedModifier * inputX;
                float newX = Mathf.SmoothDamp(
                    rb.velocity.x,
                    targetX,
                    ref _airHorzVelRef,
                    0.05f
                );

                rb.velocity = new Vector2(newX, rb.velocity.y);

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

        //CAUTION: Setting currentGravityScale here is risky business; move to conditional blocks if gives trouble
        CurrentGravityScale = rb.gravityScale;
    }

    public void SetIsGrounded(bool grounded)
    {
        _isGrounded = grounded;
    }
}
