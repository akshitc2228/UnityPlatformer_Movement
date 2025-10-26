using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //external fields:
    [Header("External fields")]
    [SerializeField] private GameObject player;
    [SerializeField] 
    private BoxCollider2D sceneBoundObj;
    [SerializeField]
    private CameraZones _zones;

    //offsets:
    [Header("Offsets for camera")]
    [SerializeField]
    private float _lateralOffset;
    [SerializeField] private float neutralVerticalOffset;
    [SerializeField]
    private float _verticalOffset;
    [SerializeField]
    float _neutralLateralOffset = 0.5f;
    [SerializeField] private float peekUpDistance = 2.5f;
    [SerializeField] private float peekDownDistance = 3.5f;

    //transition variables:
    [Header("Camera transition speeds")]
    [SerializeField]
    private float _movementTransitionSpeed = 0.125f;
    //lateral movement variables:
    [SerializeField]
    private float _directionalTransitionSpeed = 1.5f;
    [SerializeField]
    float _directionalSmoothTime;
    //vertical movement speed variables:
    [SerializeField]
    float _verticalTransitionSpeed;
    [SerializeField] private float _camVerticalVelocity;
    [SerializeField]
    private float _verticalSmoothTime;

    //local velocity refs
    private float orthographicTransitionRef = 1f;
    private float cameraMovementVelocity = 0.5f;

    float _desiredX;
    float _desiredY;
    //Idk it'd probably be best to understand clipping panes for this
    float _desiredZ = -12f;

    //fixed Offsets:
    float _forwardFacingOffset;
    float _backwardFacingOffset;

    //Camera viewport fields:
    private Camera _mainCamera;
    float _verticalExtent;
    float cameraHalfWidth;
    float _upperCameraBound;
    float _lowerCameraBound;
    //camera threshold values:
    [SerializeField]
    float _upperThreshold;
    float _lowerThreshold;

    //TODO: remove this and use the sceneBoundsSO instead
    private Bounds sceneBounds;
    private float initOrthographicSize;
    private float inputY;
    private float lockedOffsetX;
    private float lockedOffsetY;
    private bool offsetLocked = false;
    private bool isReturningFromPeek;
    private float returnTargetY;

    public float CameraClampedX { get; private set; }
    public float CameraClampedY { get; private set; }

    private GameObject playerObject;
    private Transform _playerTransform;
    private Rigidbody2D _playerRb;

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerRespawned += OnPlayerRespawned;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnPlayerRespawned -= OnPlayerRespawned;
    }

    void OnPlayerRespawned(GameObject newPlayer)
    {
        playerObject = newPlayer;
        _playerTransform = playerObject.transform;
        _playerRb = playerObject.GetComponent<Rigidbody2D>();
    }


    private void Awake()
    {
        playerObject = player;
    }

    // Start is called before the first frame update
    void Start()
    {
        _playerTransform = playerObject.transform;
        _playerRb = playerObject.GetComponent<Rigidbody2D>();

        _lateralOffset = _neutralLateralOffset;
        _verticalOffset = neutralVerticalOffset;

        _forwardFacingOffset = Mathf.Abs(_lateralOffset);
        _backwardFacingOffset = -_forwardFacingOffset;

        transform.position = new Vector3(
            _playerTransform.position.x + _lateralOffset,
            _playerTransform.position.y + _verticalOffset,
            _desiredZ
        );

        //init the camera and its initial bounds:
        _mainCamera = GetComponent<Camera>();
        _verticalExtent = _mainCamera.orthographicSize;
        initOrthographicSize = _verticalExtent;
        GetVerticalCameraBounds();
        cameraHalfWidth = _verticalExtent * Camera.main.aspect;

        //this should always be fixed
        _lowerThreshold = (_playerTransform.position.y + _verticalOffset) - _lowerCameraBound;

        //define screen bounds
        sceneBounds = sceneBoundObj.bounds;
    }

    private void Update()
    {
        inputY = Input.GetAxisRaw( "Vertical" );
        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
        {
            returnTargetY = _playerTransform.position.y + neutralVerticalOffset;
            isReturningFromPeek = true;
        }
        else if (Mathf.Abs(inputY) > 0.01f)
        {
            isReturningFromPeek = false;
        }
    }

    private void LateUpdate()
    {
        //manage orthographic size
        if (_zones && _zones.ZoneActive && _zones.CustomOrthographicSize > 0)
        {
            _mainCamera.orthographicSize = Mathf.SmoothDamp(
                _mainCamera.orthographicSize,
                _zones.CustomOrthographicSize,
                ref orthographicTransitionRef,
                _directionalSmoothTime
            );
        }
        else
        {
            _mainCamera.orthographicSize = Mathf.SmoothDamp(
                _mainCamera.orthographicSize,
                initOrthographicSize,
                ref orthographicTransitionRef,
                _directionalSmoothTime
            );
        }


        //move lateral camera:
        MoveDirectionCamera();
        _desiredX = _zones.ZoneActive ? Mathf.SmoothDamp(_desiredX, lockedOffsetX, ref _directionalTransitionSpeed, _directionalSmoothTime) : _playerTransform.position.x + _lateralOffset;

        //vertical camera movement:
        GetVerticalCameraBounds();
        _desiredY = GetCameraDesiredY();

        //Combine into desired position
        Vector3 desiredPosition = new Vector3(_desiredX, _desiredY, _desiredZ);

        //Clamp desired position BEFORE applying smoothing
        CameraClampedX = Mathf.Clamp(desiredPosition.x, sceneBounds.min.x + cameraHalfWidth, sceneBounds.max.x - cameraHalfWidth);
        CameraClampedY = Mathf.Clamp(desiredPosition.y, sceneBounds.min.y + _verticalExtent, sceneBounds.max.y - _verticalExtent);
        desiredPosition = new Vector3(CameraClampedX, CameraClampedY, _desiredZ);

        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _movementTransitionSpeed);
        transform.position = smoothedPosition;
    }


    void MoveDirectionCamera()
    {
        float facingAngleY = _playerTransform.eulerAngles.y;
        bool isFacingLeft = Mathf.Approximately(facingAngleY, 180f);

        //in fact no need for checking which way we're facing; just use velocity
        //although, we might be hit and rebounded backwards unintentionally so keep this?
        bool isMoving = Mathf.Abs(_playerRb.velocity.x) > 0.1f;

        float targetOffset = 0f;
        if (_zones && _zones.ZoneActive && _zones.XOffset != 0f)
        {
            if (!offsetLocked)
            {
                lockedOffsetX = _playerTransform.position.x + _zones.XOffset;
                offsetLocked = true;
            }

            targetOffset = _zones.XOffset;
        }
        else
        {
            offsetLocked = false;

            targetOffset = isMoving
                ? (isFacingLeft ? _backwardFacingOffset : _forwardFacingOffset)
                : _neutralLateralOffset;
        }


        _lateralOffset = Mathf.SmoothDamp(_lateralOffset, targetOffset, ref _directionalTransitionSpeed, _directionalSmoothTime);

    }

    void GetVerticalCameraBounds()
    {
        _upperCameraBound = transform.position.y + _verticalExtent;
        _lowerCameraBound = transform.position.y - _verticalExtent;
    }

    //this should return the desiredY position for the camera now or is that a misguided approach?:
    float GetCameraDesiredY()
    {
        float currentCamY = transform.position.y;
        float playerYOnly = _playerTransform.position.y;
        float playerY = playerYOnly + _verticalOffset;
        if (Mathf.Abs(inputY) > 0.01f)
        {
            if (inputY > 0)
            {
                return Mathf.SmoothDamp(currentCamY, playerY + peekUpDistance, ref _verticalTransitionSpeed, _verticalSmoothTime);
            }
            else if (inputY < 0)
            {
                return Mathf.SmoothDamp(currentCamY, playerY - peekDownDistance, ref _verticalTransitionSpeed, _verticalSmoothTime);
            }
        }

        // 2. If returning from peek (after key release), go back to neutral
        if (isReturningFromPeek)
        {
            if (Mathf.Abs(currentCamY - returnTargetY) < 0.05f)
            {
                isReturningFromPeek = false;
                return currentCamY;
            }

            return Mathf.SmoothDamp(currentCamY, returnTargetY, ref _camVerticalVelocity, _verticalSmoothTime);
        }

        float currentTopDistance = _upperCameraBound - playerY;
        float currentBottomDistance = playerY - _lowerCameraBound;

        if (currentTopDistance < _upperThreshold)
        {
            float moveBy = _upperThreshold - currentTopDistance;
            return Mathf.SmoothDamp(currentCamY, currentCamY + moveBy, ref _verticalTransitionSpeed, _verticalSmoothTime);
        }
        else if (currentBottomDistance < _lowerThreshold)
        {
            float moveBy = _lowerThreshold - currentBottomDistance;
            return Mathf.SmoothDamp(currentCamY, currentCamY - moveBy, ref _verticalTransitionSpeed, _verticalSmoothTime);
        }

        return currentCamY;
    }
}
