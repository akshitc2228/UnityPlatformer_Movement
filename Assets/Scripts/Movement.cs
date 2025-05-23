using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Movement : MonoBehaviour
{
    //movement variables:
    private float movementSpeed;
    [SerializeField]
    private float _landMovementSpeed = 8f;
    [SerializeField]
    private float _airMovementSpeed = 8f;
    [SerializeField]
    private float _maxSlideVelocity = 5f;

    //player components
    private Transform _playerTransform;
    private Rigidbody2D _playerRigidbody;

    #region FIELDS
    //ground check
    [SerializeField]
    private Transform _groundChecker;
    [SerializeField]
    private float _groundCheckerRadius = 0.32f;

    //wall sliding check:
    [SerializeField]
    private Transform _wallChecker;
    [SerializeField]
    private float _wallCheckerRadius = 0.32f;

    //jump variables
    [SerializeField]
    private float _jumpVelocity;
    [SerializeField]
    private float _maxJumpHeight = 2.5f;
    [SerializeField]
    private float _timeToApex = 0.22f;
    [SerializeField]
    private float _playerGravity;
    [SerializeField]
    private float _fallMultiplier = 0.75f;
    [SerializeField]
    private float _lowJumpMultiplier = 0.5f;

    [SerializeField]
    float _maxFallSpeed = 36f;

    //jump time variables:
    [SerializeField]
    float _jumpTimeMax = 0.15f;
    float _jumpTimeCounter = 0f;

    //coyoteTime:
    [SerializeField]
    private float _coyoteTime;
    private float _coyoteTimer;
    private bool _inCoyoteTime;

    //jump buffer:
    [SerializeField]
    float _jumpBufferTime = 0.2f;
    float _jumpBufferTimer = 0f;

    //wall jump variables:
    [SerializeField]
    float _wallJumpBufferTime = 0.1f;
    float _wallJumpBufferTimer = 0f;
    [SerializeField]
    float _wallJumpXMult = 1.2f;
    [SerializeField]
    float _wallJumpYMult = 1.1f;
    bool _isWallJumping;

    //air time:
    [SerializeField]
    float _airTime = 0.2f;
    [SerializeField]
    float _gravityReducer = 0.7f;
    float _airTimer = 0f;

    //share with the animator script
    public bool isJumping;
    public bool isWallSliding;
    #endregion

    //layers
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private LayerMask _wallLayer;

    //booleans:
    public bool isGrounded;
    public bool touchingWall;

    private void Awake()
    {
        _playerTransform = GetComponent<Transform>();
        _playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        isGrounded = false;
        //compute jump variables:
        _playerGravity = -2 * _maxJumpHeight / Mathf.Pow(_timeToApex, 2);
        _jumpVelocity = 2 * _maxJumpHeight / _timeToApex;
    }

    void Update()
    {
        //track coyote time:
        if (isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }

        //jump buffer:
        if (Input.GetKey(KeyCode.Space))
        {
            _jumpBufferTimer = _jumpBufferTime;
            _airTimer = _airTime;
        }
        else
        {
            _jumpBufferTimer -= Time.deltaTime;
            _airTimer -= Time.deltaTime;
        }

        //wall sliding; allow jumping during this phase:
        if(!isGrounded && Input.GetAxisRaw("Horizontal") != 0 && touchingWall)
        {
            isWallSliding = true;
        } 
        else
        {
            isWallSliding = false;
        }

        //perhaps consider moving these to FixedUpdate
        MovePlayer();
        PlayerJumpMovement();
        WallSlide();
        WallJump();
    }

    private void FixedUpdate()
    {
        //may need to replace overlapcircle with overlap all in case there's multiple colliders
        isGrounded = Physics2D.OverlapCircle(_groundChecker.transform.position, _groundCheckerRadius, _groundLayer);

        //here rather than use ground layer perhaps attack infintesimally thing game objects to platoform edges and call it a separate layer?
        touchingWall = Physics2D.OverlapCircle(_wallChecker.transform.position, _wallCheckerRadius, _wallLayer);
        PlayerJumpPhysics();
    }

    void MovePlayer()
    {
        float horizontalDirection = Input.GetAxis("Horizontal");

        if (horizontalDirection < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        else if (horizontalDirection > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);

        //change speed during airTime
        //if(!isGrounded && _airTimer > 0f && Mathf.Abs(_playerRigidbody.velocity.y) < 0.1f)
        //{
        //    movementSpeed = _airMovementSpeed;
        //}
        //else
        //{
        //    movementSpeed = _landMovementSpeed;
        //}

        //dont need to multiply rigidbody props with time delta time; they are inherently frame rate independent
        //transition between land and air speeds is not very fluid atm:
        _playerRigidbody.velocity = new Vector2(horizontalDirection * _landMovementSpeed, _playerRigidbody.velocity.y);
    }

    void PlayerJumpMovement()
    {
        // Start jump
        if (Input.GetKey(KeyCode.Space) && _jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            isJumping = true;
            _jumpTimeCounter = _jumpTimeMax;
        }

        // Continue jump while holding
        if (_jumpBufferTimer > 0f && isJumping)
        {
            _coyoteTimer = 0f;
            _jumpBufferTimer = 0f;
            if (_jumpTimeCounter > 0)
            {
                _jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // End jump early (short hop)
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
            //NOTE: reduce jump time counter here as well no?
            if (_airTimer > 0f) _airTimer -= Time.deltaTime;
        }
    }

    void WallSlide()
    {
        //wall sliding speed:
        if (isWallSliding)
        {
            _isWallJumping = false;
            _wallJumpBufferTimer = _wallJumpBufferTime;
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, Mathf.Clamp(_playerRigidbody.velocity.y, -_maxSlideVelocity, float.MaxValue));

            CancelInvoke(nameof(StopWallJump));
        }
        else
        {
            _wallJumpBufferTimer -= Time.deltaTime;
        }
    }

    void WallJump()
    {
        //need to give direction here as well
        float jumpDirection = Input.GetAxisRaw("Horizontal");

        if(isWallSliding && _wallJumpBufferTimer > 0f && Input.GetKeyDown(KeyCode.Space))
        {
            _isWallJumping = true;
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x * _wallJumpXMult * jumpDirection, _jumpVelocity * _wallJumpYMult);
            _wallJumpBufferTimer = 0f;
        }

        //put the delay into a variable
        Invoke(nameof(StopWallJump), 0.2f);
    }

    void StopWallJump()
    {
        _isWallJumping = false;
    }


    void PlayerJumpPhysics()
    {
        // Apply upward velocity while jump is held and time remains
        if (isJumping && _jumpTimeCounter > 0 && _playerRigidbody.velocity.y <= _jumpVelocity)
        {
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, _jumpVelocity);
        }

        // Apply low jump multiplier if jump released early
        if (isJumping && Input.GetKeyUp(KeyCode.Space))
        {
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, _jumpVelocity * _lowJumpMultiplier);
        }

        // Apex hover: reduce gravity when nearly stationary in air
        if (!isGrounded && Mathf.Abs(_playerRigidbody.velocity.y) < 0.1f && _airTimer > 0f)
        {
            _playerRigidbody.gravityScale = (_playerGravity / Physics2D.gravity.y) * _gravityReducer;

            //give a slight increase to player horizontal speed to allow more control in the air
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x * 1.5f, _playerRigidbody.velocity.y);
        }
        // Falling: increase gravity with fall multiplier
        else if (_playerRigidbody.velocity.y < 0 && _airTimer <= 0f)
        {
            _playerRigidbody.gravityScale = (_playerGravity / Physics2D.gravity.y) * _fallMultiplier;
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, Mathf.Max(_playerRigidbody.velocity.y, -_maxFallSpeed));
        }
        // Default gravity when not jumping/falling
        else
        {
            _playerRigidbody.gravityScale = _playerGravity / Physics2D.gravity.y;
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (_groundChecker != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_groundChecker.transform.position, _groundCheckerRadius);
        }

        if(_wallChecker != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_wallChecker.transform.position, _wallCheckerRadius);
        }
    }
}