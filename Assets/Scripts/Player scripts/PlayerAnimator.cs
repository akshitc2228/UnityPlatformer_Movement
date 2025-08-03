using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator playerAnimator;
    [SerializeField]
    private float hurtAnimationDuration;

    private PlayerController controller;
    private PlayerMovement movement;
    private PlayerHealth playerHealth;

    private Rigidbody2D playerRb;
    private bool dashTriggerSetThisFrame = false;
    private bool hurtTriggerSetThisFrame = false;

    //boolean flagNames for animation:
    private static readonly int xVelocity = Animator.StringToHash("xVelocity");
    private static readonly int yVelocity = Animator.StringToHash("yVelocity");
    private static readonly int GroundedFlag = Animator.StringToHash("isGrounded");
    private static readonly int CrouchFlag = Animator.StringToHash("IsCrouching");
    private static readonly int DashTrigger = Animator.StringToHash("dashPressed");
    private static readonly int HurtFlag = Animator.StringToHash("playerHurt");

    private Coroutine hurtAnimationCoroutine;

    private void OnEnable()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerHealth.OnPlayerHurt += PlayHurtAnimation;
    }

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
        if (movement.IsDashing && !dashTriggerSetThisFrame)
        {
            playerAnimator.SetTrigger(DashTrigger);
            dashTriggerSetThisFrame = true;
        }
        if (!movement.IsDashing)
        {
            dashTriggerSetThisFrame = false;
        }
    }

    IEnumerator endHurtAnimation()
    {
        //replace this with a const which defines the invincibility frames
        yield return new WaitForSeconds(hurtAnimationDuration);
        playerAnimator.SetBool(HurtFlag, false);
        //hurtTriggerSetThisFrame = false;
        hurtAnimationCoroutine = null;
    }

    private void PlayHurtAnimation()
    {
        if (!hurtTriggerSetThisFrame)
        {
            playerAnimator.SetBool(HurtFlag, true);
            //hurtTriggerSetThisFrame = true;
            if(hurtAnimationCoroutine == null)
                StartCoroutine(endHurtAnimation());
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnPlayerHurt -= PlayHurtAnimation;
    }

}
