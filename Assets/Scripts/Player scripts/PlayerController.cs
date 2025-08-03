using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("ground check variables")]
    [SerializeField] private Transform groundChecker;
    [SerializeField] private float groundCheckerRadius = 0.18f;
    [SerializeField] private PhysicsLayerProfile physicsLayers;

    [Header("Damage variables")]
    [SerializeField] private float knockbackForceX = 10f;
    [SerializeField] private float knockbackForceY = 10f;
    [SerializeField] private float knockbackVelSmoothTime = 0.2f;
    //right now consistent with the time in which the trigger resets
    [SerializeField] private float damageModeTimer = 0.2f;

    private bool isHurt = false;
    private Coroutine playerHurtCoroutine;
    private Vector2 knockbackVelocity;
    private Vector2 knockbackVelocityRef;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerJump jump;
    private PlayerWallInteraction wall;
    private PlayerHealth playerHealth;

    private float inputX;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool dashPressed;
    private bool crouchHeld;
    private bool crouchReleased;

    public bool IsGrounded {  get; private set; }

    private void OnEnable()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerHealth.OnPlayerHurt += KnockBackPlayer;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<PlayerJump>();
        wall = GetComponent<PlayerWallInteraction>();
    }

    void Update()
    {
        //A KEY PERSONAL NOTE REGARDING INPUTS:
        /**
         * GetKeyDown fires only for 1 frame when the key is first pressed;
         * If you press RightShift slightly before a physics frame (FixedUpdate), the dash logic (and other logics) might miss it entirely
         * The input reading should also have happened in FixedUpdate I think;
         * OR use GetKey for multiple frames and give the method enough time to react;
         * OR consider buffering movement functions
         **/
        movement.SetMovementDirection(inputX);
        if (isHurt) return;

        crouchHeld = Input.GetKey(KeyCode.LeftShift);
        inputX = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        jumpHeld = Input.GetKey(KeyCode.Space);
        dashPressed = Input.GetKey(KeyCode.J);

        if(!movement.IsDashing)
        {
            wall.TryWallJump(jumpPressed);
            jump.HandleJumpInput(jumpPressed, jumpHeld);
        }
    }

    void FixedUpdate()
    {
        //some of these would need to be carefully scoped in an if where isHurt is true; in that scope dont trigger those functions
        //in fact we need to do a similar logic for animation as well probs but one at a time
        IsGrounded = Physics2D.OverlapCircle(groundChecker.transform.position, groundCheckerRadius, physicsLayers.groundLayer | physicsLayers.hybridLayer);
        movement.CheckDirectionFacing(inputX);
        wall.SetGravityReference(jump.CurrentGravityScale);
        jump.SetIsGrounded(IsGrounded);

        if (isHurt) return;

        movement.DashPlayer(dashPressed, rb);

        jump.ApplyJumpPhysics(rb, inputX);

        wall.HandleWallSlide(inputX, IsGrounded);

        if (!movement.IsDashing)
        {
            movement.Move(rb);
            if (IsGrounded)
                movement.Crouch(crouchHeld, rb, jumpHeld, dashPressed);
        }

        if(isHurt)
        {
            knockbackVelocity = Vector2.SmoothDamp(knockbackVelocity, Vector2.zero, ref knockbackVelocityRef, knockbackVelSmoothTime);
            rb.velocity += knockbackVelocity;
        }
    }

    //throw the player back a bit when attacked
    void KnockBackPlayer()
    {
        if (isHurt) return;

        isHurt = true;

        int directionFacing = Mathf.Approximately(transform.eulerAngles.y, 180f) ? -1 : 1;

        Vector2 knockbackDirection = new Vector2(-directionFacing * knockbackForceX, knockbackForceY);
        knockbackVelocity = knockbackDirection;

        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection, ForceMode2D.Impulse);

        if (playerHurtCoroutine == null)
           StartCoroutine(RemoveHurtEventLock());
    }

    IEnumerator RemoveHurtEventLock()
    {
        yield return new WaitForSeconds(damageModeTimer);
        isHurt = false;
        playerHurtCoroutine = null;
    }


    //DEBUG GIZMO
    private void OnDrawGizmosSelected()
    {
        if(groundChecker != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundChecker.transform.position, groundCheckerRadius);
        }
    }

    private void OnDisable()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
            playerHealth.OnPlayerHurt -= KnockBackPlayer;
    }
}
