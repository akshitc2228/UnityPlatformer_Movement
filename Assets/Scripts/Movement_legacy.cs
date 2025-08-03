using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

//REFACTORING: look into something called headers
public class Movement : MonoBehaviour
{
    //movement variables:
    private float movementSpeed;
    [SerializeField]
    private float _landMovementSpeed = 8f;
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
    private float _wallCheckerDist = 1.5f;

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

    //jump buffer:
    [SerializeField]
    float _jumpBufferTime = 0.2f;
    float _jumpBufferTimer = 0f;

    //wall jump variables:
    [SerializeField]
    float _wallJumpBufferTime = 0.2f;
    float _wallJumpBufferTimer = 0f;

    float _wallJumpDirection;

    [SerializeField]
    float _wallCoyoteTime = 0.1f;
    float _wallCoyoteTimer = 0f;

    [SerializeField]
    Vector2 _wallJumpForce;
    bool _isWallJumping;

    Coroutine _setWallJumpBufferCoroutine;

    //air time:
    [SerializeField]
    float _airTime = 0.7f;
    [SerializeField]
    float _gravityReducer = 0.5f;
    float _airTimer = 0f;
    [SerializeField]
    float _airSpeedModifier = 1.5f;
    private float _airHorzVelRef = 1.5f;

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
            _airTimer = _airTime;
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
        if(!_isWallJumping)
        {
            MovePlayer();
        }
        PlayerJumpMovement();
        WallSlide();
        WallJump();
    }

    private void FixedUpdate()
    {
        //may need to replace overlapcircle with overlap all in case there's multiple colliders
        isGrounded = Physics2D.OverlapCircle(_groundChecker.transform.position, _groundCheckerRadius, _groundLayer);

        touchingWall = Physics2D.Raycast(_wallChecker.position, Vector2.right * transform.localScale.x, _wallCheckerDist, _wallLayer);

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
            //maybe reduce jumpTime counter?
        }
    }

    void WallSlide()
    {
        if (isWallSliding)
        {
            _isWallJumping = false;
            _playerRigidbody.velocity = new Vector2(
                _playerRigidbody.velocity.x,
                Mathf.Clamp(_playerRigidbody.velocity.y, -_maxSlideVelocity, float.MaxValue)
            );

            _wallCoyoteTimer = _wallCoyoteTime;

            _wallJumpDirection = (transform.eulerAngles.y == 180) ? -1 : 1;

            // Only start buffer timer if not already doing it
            if (_setWallJumpBufferCoroutine == null)
                _setWallJumpBufferCoroutine = StartCoroutine(DelayedSetWallJumpBuffer());
        }
        else
        {
            _wallCoyoteTimer -= Time.deltaTime;
            _wallJumpBufferTimer -= Time.deltaTime;
            // Don't kill the coroutine here unless you have good reason to.
        }
    }

    IEnumerator DelayedSetWallJumpBuffer()
    {
        yield return new WaitForSeconds(0.05f); // Short delay to prevent instant repeat jumps
        _wallJumpBufferTimer = _wallJumpBufferTime;
        _setWallJumpBufferCoroutine = null;
    }

    void WallJump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && _wallJumpBufferTimer > 0f && _wallCoyoteTimer > 0f)
        {
            //before anything freeze the player's speeds midair and then initiate the jumping; this will use a coroutine as well.

            _isWallJumping = true;

            _playerRigidbody.velocity = new Vector2(_wallJumpForce.x * _wallJumpDirection, _wallJumpForce.y);
            _wallJumpBufferTimer = 0f;
            _wallCoyoteTimer = 0f;
        }
        else
        {
            _isWallJumping = false;
        }
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

        if (!isGrounded && Mathf.Abs(_playerRigidbody.velocity.y) < 2f && _airTimer > 0f)
        {
            //Debug.Log($"AIR TIME! gravity now: {_playerRigidbody.gravityScale}");
            _playerRigidbody.gravityScale = (_playerGravity / Physics2D.gravity.y) * _gravityReducer;

            float inputX = Input.GetAxisRaw("Horizontal");
            float targetX = _playerRigidbody.velocity.x * _airSpeedModifier * inputX;
            float newX = Mathf.SmoothDamp(
                _playerRigidbody.velocity.x,
                targetX,
                ref _airHorzVelRef,
                0.05f
            );

            _playerRigidbody.velocity = new Vector2(newX, _playerRigidbody.velocity.y);

            _airTimer = Mathf.Max(0f, _airTimer - Time.fixedDeltaTime);

        }
        else if (_playerRigidbody.velocity.y < 0 && _airTimer <= 0f)
        {
            //Debug.Log("over");
            _playerRigidbody.gravityScale = (_playerGravity / Physics2D.gravity.y) * _fallMultiplier;
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, Mathf.Max(_playerRigidbody.velocity.y, -_maxFallSpeed));
        }
        else
        {
            _playerRigidbody.gravityScale = _playerGravity / Physics2D.gravity.y;
            //Debug.Log($"applying usual gravity: {_playerRigidbody.gravityScale}");
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
            Gizmos.DrawRay(_wallChecker.position, Vector2.right * transform.localScale.x * _wallCheckerDist);
        }
    }
}