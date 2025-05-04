using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator _playerAnimator;
    private Movement _movementScript;

    private Rigidbody2D _playerRb;

    //boolean flagNames for animation:
    string groundedFlag = "isGrounded";
    string jumpingFlag = "isJumping";
    string fallingFlag = "isFalling";

    float verticalVelocity;

    private void Awake()
    {
        _playerAnimator = GetComponent<Animator>();
        _movementScript = GetComponent<Movement>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        verticalVelocity = _playerRb.velocity.y;
        SwitchAnimations();
    }

    public void SwitchAnimations()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        //transition to running
        if(horizontalInput != 0 )
        {
            _playerAnimator.SetBool("isRunning", true);
        } 
        else
        {
            _playerAnimator.SetBool("isRunning", false);
        }

        //transition to jumping
        //if isJumping and velocity is greater than 0
        if(!_movementScript.isGrounded && verticalVelocity > 0.1f)
        {
            _playerAnimator.SetBool(groundedFlag, false);
            _playerAnimator.SetBool(jumpingFlag, true);
        }
        //possible cause of concerm is that grounded flag is not used and isJumping is not used when
        //transitioning to descent animation
        else if(!_movementScript.isGrounded && verticalVelocity <= -0.1f)
        {
            _playerAnimator.SetBool(groundedFlag, false);
            _playerAnimator.SetBool(jumpingFlag, false);
            _playerAnimator.SetBool(fallingFlag, true);
        }
        else
        {
            _playerAnimator.SetBool(groundedFlag, true);
            _playerAnimator.SetBool(jumpingFlag, false);
            _playerAnimator.SetBool(fallingFlag, false);
        }
    }
}
