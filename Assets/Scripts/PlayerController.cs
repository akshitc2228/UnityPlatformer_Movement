using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("ground check variables")]
    [SerializeField] private Transform groundChecker;
    [SerializeField] private float groundCheckerRadius = 0.18f;
    [SerializeField] private PhysicsLayerProfile physicsLayers;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerJump jump;
    private PlayerWallInteraction wall;

    private float inputX;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool dashPressed;
    private bool crouchHeld;
    private bool crouchReleased;

    public bool IsGrounded {  get; private set; }

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

        crouchHeld = Input.GetKey(KeyCode.LeftShift);
        inputX = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        jumpHeld = Input.GetKey(KeyCode.Space);
        dashPressed = Input.GetKey(KeyCode.J);

        if(!movement.IsDashing)
        {
            wall.TryWallJump(jumpPressed);
            //Debug.Log("exit wall jump");
            jump.HandleJumpInput(jumpPressed, jumpHeld);
        }
        movement.SetMovementDirection(inputX);
    }

    void FixedUpdate()
    {
        IsGrounded = Physics2D.OverlapCircle(groundChecker.transform.position, groundCheckerRadius, physicsLayers.groundLayer | physicsLayers.hybridLayer);
        movement.CheckDirectionFacing(inputX);
        movement.DashPlayer(dashPressed, rb);

        jump.ApplyJumpPhysics(rb, inputX);

        wall.SetGravityReference(jump.CurrentGravityScale);
        wall.HandleWallSlide(inputX, IsGrounded);

        jump.SetIsGrounded(IsGrounded);

        if (!movement.IsDashing)
        {
            movement.Move(rb);
            if (IsGrounded)
                movement.Crouch(crouchHeld, rb, jumpHeld, dashPressed);
        }

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
}
