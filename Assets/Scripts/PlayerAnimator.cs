using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator playerAnimator;
    private PlayerController controller;
    private PlayerMovement movement;

    private Rigidbody2D playerRb;

    //boolean flagNames for animation:
    private static readonly int xVelocity = Animator.StringToHash("xVelocity");
    private static readonly int yVelocity = Animator.StringToHash("yVelocity");
    private static readonly int GroundedFlag = Animator.StringToHash("isGrounded");
    private static readonly int CrouchFlag = Animator.StringToHash("IsCrouching");
    private static readonly int DashTrigger = Animator.StringToHash("dashPressed");

    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        movement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        playerAnimator.SetFloat(xVelocity, Mathf.Abs(playerRb.velocity.x));
        playerAnimator.SetFloat(yVelocity, playerRb.velocity.y);
        playerAnimator.SetBool(GroundedFlag, controller.IsGrounded);
        playerAnimator.SetBool(CrouchFlag, movement.IsCrouching);
        if(movement.IsDashing) 
            playerAnimator.SetTrigger(DashTrigger);
    }
}
