using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallInteraction : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField] private float wallSlideSpeed = 5f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(20f, 30f);
    [SerializeField] private float wallJumpBufferTime = 0.2f;
    [SerializeField] private float wallCoyoteTime = 0.1f;
    [SerializeField] private PhysicsProfile physics;
    [SerializeField] private Transform wallChecker;
    [SerializeField] private float wallCheckerOffset = 0.5f;
    [SerializeField] private float wallCheckDistance = 0.6f;

    private float wallJumpBufferTimer;
    private float wallCoyoteTimer;
    private int wallJumpDirection;
    private Coroutine delayJumpCoroutine;
    private Coroutine wallJumpFreezeRoutine;
    private Rigidbody2D rb;

    private Vector2 wallCheckOrigin;
    private Vector2 wallCheckDirection;
    public bool TouchingWall { get; private set; }
    public bool IsWallJumping { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void HandleWallSlide(bool IsGrounded, float inputX)
    {
        wallCheckOrigin = (transform.eulerAngles.y == 180f) ? (Vector2)wallChecker.position - new Vector2(wallCheckerOffset, 0) :
            wallChecker.position;

        wallCheckDirection = (transform.eulerAngles.y == 180f) ? Vector2.left : Vector2.right;

        TouchingWall = Physics2D.Raycast(wallCheckOrigin, wallCheckDirection, wallCheckDistance, physics.wallLayer | physics.hybridLayer);

        //CAUTION: removed manadatory input from this conditional; CHECK IF BUGS
        if (TouchingWall && !IsGrounded)
        {
            wallCoyoteTimer = wallCoyoteTime;

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
            wallJumpDirection = (Mathf.Abs(inputX) > 0.01f) ? (int) Mathf.Sign(inputX) : wallCheckDirection == Vector2.right ? -1 : 1;

            if (delayJumpCoroutine == null)
                delayJumpCoroutine = StartCoroutine(DelayedWallJumpBuffer());
        }
        else
        {
            wallCoyoteTimer -= Time.deltaTime;
            wallJumpBufferTimer -= Time.deltaTime;
        }
    }

    public void TryWallJump(bool jumpPressed)
    {
        if (jumpPressed && wallJumpBufferTimer > 0f && wallCoyoteTimer > 0f)
        {
            if (wallJumpFreezeRoutine == null)
            {
                wallJumpFreezeRoutine = StartCoroutine(WallJumpFreezeCoroutine());
            }
        }
    }

    IEnumerator DelayedWallJumpBuffer()
    {
        yield return new WaitForSeconds(0.05f);
        wallJumpBufferTimer = wallJumpBufferTime;
        delayJumpCoroutine = null;
    }

    IEnumerator WallJumpFreezeCoroutine()
    {
        IsWallJumping = true;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;

        yield return new WaitForSeconds(0.05f); // Freeze effect

        rb.gravityScale = physics.GlobalGravityScaleReference; // Reapply normal gravity scale, or pull from a gravity setting
        rb.velocity = new Vector2(
            wallJumpForce.x * wallJumpDirection,
            wallJumpForce.y
        );
        IsWallJumping = false;

        wallJumpBufferTimer = 0f;
        wallCoyoteTimer = 0f;
        wallJumpFreezeRoutine = null;
    }

    //DEBUG GIZMO
    private void OnDrawGizmosSelected()
    {
        if(wallChecker != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(wallCheckOrigin, wallCheckDirection * wallCheckDistance);
        }
    }
}
