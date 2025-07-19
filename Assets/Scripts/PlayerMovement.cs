using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float lateralSpeedSmoothness = 0.5f;
    [SerializeField] private float landMovementSpeed = 8f;
    [SerializeField] private float crouchSpeedModifier = 0.5f;
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.2f;

    public bool IsDashing {  get; private set; }
    public bool IsCrouching { get; private set; }

    private bool canDash = true;
    private float _directionFacing;

    private Coroutine dashCoroutine;
    private PlayerJump jumpScript;
    private BoxCollider2D playerCollider;

    private Vector2 defaultColliderSize;
    private Vector2 defaultColliderOffset;
    private static readonly Vector2 crouchColliderOffsets = new Vector2(0.0819392204f, -0.392683148f);
    private static readonly Vector2 crouchColliderSize = new Vector2(1.25884533f, 1.20767879f);

    private void Start()
    {
        jumpScript = GetComponent<PlayerJump>();
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

    public void Move(Rigidbody2D rb)
    {
        float smoothedSpeed = Mathf.SmoothStep(rb.velocity.x, landMovementSpeed * _directionFacing, lateralSpeedSmoothness);
        rb.velocity = new Vector2(smoothedSpeed, rb.velocity.y);
    }

    public void DashPlayer(bool dashPressed, Rigidbody2D rb)
    {
        if (dashPressed && !IsDashing && dashCoroutine == null && canDash)
        {
            IsDashing = true;
            canDash = false;

            rb.gravityScale = 0f;
            float dashDir = (_directionFacing != 0) ? Mathf.Sign(_directionFacing) : Mathf.Sign(transform.right.x);

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
            float smoothedCrouchSpeed = Mathf.SmoothStep(rb.velocity.x, _directionFacing * landMovementSpeed * crouchSpeedModifier, lateralSpeedSmoothness);

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
        rb.gravityScale = jumpScript.CurrentGravityScale;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
        dashCoroutine = null;
    }

    public void SetMovementDirection(float inputX)
    {
        _directionFacing = inputX;
    }
}
