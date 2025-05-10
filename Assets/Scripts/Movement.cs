using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    //movement variables:
    private float movementSpeed;
    [SerializeField]
    private float landMovementSpeed = 8f;
    [SerializeField]
    private float airMovementSpeed = 8f;

    //player components
    private Transform _playerTransform;
    private Rigidbody2D _playerRigidbody;

    #region JUMP VARIABLES
    [SerializeField]
    private Transform _groundChecker;
    [SerializeField]
    private float _groundCheckerRadius = 0.32f;

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

    //air time:
    [SerializeField]
    float _airTime = 0.2f;
    [SerializeField]
    float _gravityReducer = 0.7f;
    float _airTimer = 0f;

    //share with the animator script
    public bool _isJumping;
    #endregion

    //layers
    [SerializeField]
    private LayerMask _groundLayer;

    //booleans:
    public bool isGrounded;

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

        MovePlayer();
        PlayerJumpMovement();
    }

    private void FixedUpdate()
    {
        //before checking the overlap; check if out o coyote time
        isGrounded = Physics2D.OverlapCircle(_groundChecker.transform.position, _groundCheckerRadius, _groundLayer);
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
        //    movementSpeed = airMovementSpeed;
        //}
        //else
        //{
        //    movementSpeed = landMovementSpeed;
        //}

        //dont need to multiply rigidbody props with time delta time; they are inherently frame rate independent
        //transition between land and air speeds is not very fluid atm:
        _playerRigidbody.velocity = new Vector2(horizontalDirection * landMovementSpeed, _playerRigidbody.velocity.y);
    }

    void PlayerJumpMovement()
    {
        // Start jump
        if (Input.GetKey(KeyCode.Space) && _jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            _isJumping = true;
            _jumpTimeCounter = _jumpTimeMax;
        }

        // Continue jump while holding
        if (_jumpBufferTimer > 0f && _isJumping)
        {
            _coyoteTimer = 0f;
            _jumpBufferTimer = 0f;
            if (_jumpTimeCounter > 0)
            {
                _jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                _isJumping = false;
            }
        }

        // End jump early (short hop)
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _isJumping = false;
            if (_airTimer > 0f) _airTimer -= Time.deltaTime;
        }
    }


    void PlayerJumpPhysics()
    {
        // Apply upward velocity while jump is held and time remains
        if (_isJumping && _jumpTimeCounter > 0 && _playerRigidbody.velocity.y <= _jumpVelocity)
        {
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, _jumpVelocity);
        }

        // Apply low jump multiplier if jump released early
        if (_isJumping && Input.GetKeyUp(KeyCode.Space))
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
    }
}