using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float lateralSpeedSmoothness = 0.5f;
    [SerializeField] private float landMovementSpeed = 8f;
    [SerializeField] private float crouchSpeedModifier = 0.5f;
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.2f;
    [SerializeField] private float groundedStopDamp = 15f;

    [Header("Sliding params")]
    [SerializeField] private float maxSlideSpeed = 30f;
    [SerializeField] private float slideModifier = 1.8f;

    [SerializeField] private PhysicsProfile physics;

    public bool IsDashing {  get; private set; }
    public bool IsCrouching { get; private set; }

    private bool canDash = true;
    public float DirectionFacing { get; private set; }

    private Coroutine dashCoroutine;
    private BoxCollider2D playerCollider;

    private Vector2 defaultColliderSize;
    private Vector2 defaultColliderOffset;

    //this should be part of a scriptable object
    private static readonly Vector2 crouchColliderOffsets = new Vector2(0.0819392204f, -0.392683148f);
    private static readonly Vector2 crouchColliderSize = new Vector2(1.25884533f, 1.20767879f);

    //ref for smoothDamp in the move method
    private float velocityXSmooth;

    private void Start()
    {
        playerCollider = GetComponent<BoxCollider2D>();
        defaultColliderSize = playerCollider.size;
        defaultColliderOffset = playerCollider.offset;
    }

    public void CheckDirectionFacing(float _directionFacing)
    {
        if (_directionFacing < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (_directionFacing > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    public void Move(Rigidbody2D rb, Vector2 groundNormal, bool isGrounded, float inputX)
    {
        if (!isGrounded) return; // leave airborne motion to JumpPhysics

        // 1. Calculate slope tangent
        Vector2 groundTangent = new Vector2(groundNormal.y, -groundNormal.x).normalized;

        // 2. Project current velocity onto tangent
        float velocityTangent = Vector2.Dot(rb.velocity, groundTangent);
        float tangentSpeed = velocityTangent;

        float slopeAngle = Vector2.Angle(groundNormal, Vector2.up);

        ////for when we wanna snap stop
        //if (isGrounded && Mathf.Abs(inputX) < 0.01f)
        //{
        //    // simulate static friction by damping horizontal velocity
        //    rb.velocity = new Vector2(
        //        Mathf.Lerp(rb.velocity.x, 0, Time.fixedDeltaTime * groundedStopDamp),
        //        rb.velocity.y);
        //}

        if (slopeAngle <= 0.1f)
        {
            float targetX = DirectionFacing * landMovementSpeed;
            float smoothed = Mathf.SmoothDamp(rb.velocity.x, targetX, ref velocityXSmooth, lateralSpeedSmoothness);
            rb.velocity = new Vector2(smoothed, rb.velocity.y);
            return; // short-circuit, don’t apply tangent logic
        }
        else
        {
            // Slope: auto-slide
            float slideDir = Mathf.Sign(Vector2.Dot(Vector2.down, groundTangent));
            tangentSpeed += slideDir * slideModifier * Time.fixedDeltaTime;
            tangentSpeed = Mathf.Clamp(tangentSpeed, -maxSlideSpeed, maxSlideSpeed);
        }

        // Apply slope-aware X velocity
        Vector2 tangentVelocity = groundTangent * tangentSpeed;
        rb.velocity = new Vector2(tangentVelocity.x, rb.velocity.y);
    }

    public void DashPlayer(bool dashPressed, Rigidbody2D rb)
    {
        if (dashPressed && !IsDashing && dashCoroutine == null && canDash)
        {
            IsDashing = true;
            canDash = false;

            rb.gravityScale = 0f;
            float dashDir = (DirectionFacing != 0) ? Mathf.Sign(DirectionFacing) : Mathf.Sign(transform.right.x);

            rb.velocity = new Vector2(dashDir * dashSpeed, 0f);
            if(dashCoroutine == null) dashCoroutine = StartCoroutine(StopDashing(rb));
        }
    }


    public void Crouch(bool crouchPressed, Rigidbody2D rb, bool jumpPressed, bool dashPressed)
    {
        if ((jumpPressed || dashPressed) && IsCrouching)
        {
            IsCrouching = false;
            playerCollider.size = defaultColliderSize;
            playerCollider.offset = defaultColliderOffset;
            return;
        }

        if (crouchPressed && !IsCrouching)
        {
            IsCrouching = true;
            float smoothedCrouchSpeed = Mathf.SmoothStep(rb.velocity.x, DirectionFacing * landMovementSpeed * crouchSpeedModifier, lateralSpeedSmoothness);

            playerCollider.offset = crouchColliderOffsets;
            playerCollider.size = crouchColliderSize;

            rb.velocity = new Vector2(smoothedCrouchSpeed, rb.velocity.y);
        }
        else if (!crouchPressed && IsCrouching)
        {
            IsCrouching = false;
            playerCollider.size = defaultColliderSize;
            playerCollider.offset = defaultColliderOffset;
        }
    }

    private IEnumerator StopDashing(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(dashDuration);

        IsDashing = false;
        rb.gravityScale = physics.GlobalGravityScaleReference;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
        dashCoroutine = null;
    }

    public void SetMovementDirection(float inputX)
    {
        DirectionFacing = inputX;
    }
}
