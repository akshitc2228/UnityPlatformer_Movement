using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private float movementSpeed = 8f;

    //player components
    private SpriteRenderer _playerSr;
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

    //jump time variables; controls how long to jump:
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
        _playerSr = GetComponent<SpriteRenderer>();
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
        if(isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }

        //jump buffer:
        //TODO: something to notice here is that airTime is always set to max when space is pressed
        //it reduces when you reach maxheight but when you add double jump...what would happen?
        if(Input.GetKey(KeyCode.Space))
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
        //before checking the overlap; check if out of coyote time
        isGrounded = Physics2D.OverlapCircle(_groundChecker.transform.position, _groundCheckerRadius, _groundLayer);
        PlayerJumpPhysics();
    }

    void MovePlayer()
    {
        float horizontalDirection = Input.GetAxis("Horizontal");

        if (horizontalDirection < 0)
            _playerSr.flipX = true;
        else if (horizontalDirection > 0)
            _playerSr.flipX = false;

        //dont need to multiply rigidbody props with time delta time; they are inherently frame rate independent
        _playerRigidbody.velocity = new Vector2(horizontalDirection * movementSpeed, _playerRigidbody.velocity.y);
    }

    void PlayerJumpMovement()
    {
        // Start jump
        //this here causes problems when using bufferTime; in that case use simple getKey
        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            _isJumping = true;
            _jumpTimeCounter = _jumpTimeMax;
            _jumpBufferTimer = 0f;
        }

        // Continue jump while holding
        if (_jumpBufferTimer > 0f && _isJumping)
        {
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
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
        if (_isJumping && Input.GetKeyUp(KeyCode.Space))
        {
            _coyoteTimer = 0f;
            _isJumping = false;
            if(_airTimer > 0f) _airTimer -= Time.deltaTime;
        }
    }


    void PlayerJumpPhysics()
    {
        //body velocity:
        if(_isJumping && _jumpTimeCounter > 0f && _playerRigidbody.velocity.y <= _jumpVelocity)
        {
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, _jumpVelocity);
        }
        //make fall snappy by adding fall multiplier; consider removing in the future:
        if(_isJumping && Input.GetKeyUp(KeyCode.Space))
        {
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, _jumpVelocity * _lowJumpMultiplier);
        }

        //airTime; stops mid-air so only when jumping and velocity is 0 as you start downward descent the condition should fail
        //reduce gravity so long as airTimer is > 0 and start decrementing the timer
        //if (!isGrounded && Mathf.Abs(_playerRigidbody.velocity.y) < 2f && _airTimer > 0f)
        //{
        //    Debug.Log(_playerRigidbody.velocity.y);
        //    Debug.Log(_airTimer);
        //    Debug.Log("exactly in the right condition");
        //    Debug.Log($"Reducing gravity to: {(_playerGravity / Physics2D.gravity.y) * _gravityReducer}");
        //    _playerRigidbody.gravityScale = (_playerGravity / Physics2D.gravity.y) * _gravityReducer;
        //    _airTimer -= Time.fixedDeltaTime;
        //}

        if (_playerRigidbody.velocity.y < 0)
        {
            _playerRigidbody.gravityScale = (_playerGravity / Physics2D.gravity.y) * _fallMultiplier;
            //clamp falling speed:
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, Mathf.Max(_playerRigidbody.velocity.y, -_maxFallSpeed));
        }
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
