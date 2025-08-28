using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator playerAnimator;
    [SerializeField]
    private float hurtAnimationDuration;
    [SerializeField] private float postDeathPause;

    [SerializeField] private FreezeInputEventSO freezeInputEvent;

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
    private static readonly int HurtFlag = Animator.StringToHash("playerHurt");

    private static readonly int DeathTrigger = Animator.StringToHash("triggerDeath");
    private static readonly int DashTrigger = Animator.StringToHash("dashPressed");

    private Coroutine hurtAnimationCoroutine;
    private Coroutine deathPauseCoroutine;

    /**
     * Not the best idea; introduces coupling between animator and UI script
     * refactor somehow and use a separate events handler to queue tasks maybe
     * **/
    public event Action ShowGameOverMenu;

    private void OnEnable()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerHealth.OnPlayerHurt += PlayHurtAnimation;
        playerHealth.OnPlayerDeath += PlayDeathAnimation;
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
        yield return new WaitForSeconds(hurtAnimationDuration);
        playerAnimator.SetBool(HurtFlag, false);
        hurtAnimationCoroutine = null;
    }

    IEnumerator pauseAfterDeath()
    {
        yield return new WaitForSecondsRealtime(postDeathPause);
        playerAnimator.ResetTrigger(DeathTrigger);
        deathPauseCoroutine = null;

        ShowGameOverMenu?.Invoke();
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

    private void PlayDeathAnimation()
    {
        //freeze all input first
        freezeInputEvent.Raise(true);
        playerAnimator.SetTrigger(DeathTrigger);
        if(deathPauseCoroutine == null)
        {
            StartCoroutine(pauseAfterDeath());
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerHurt -= PlayHurtAnimation;
            playerHealth.OnPlayerDeath -= PlayDeathAnimation;
        }
    }

}
