using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//THIS FILE IS BAD CODING; PLEASE FIX IT
public class CameraController : MonoBehaviour
{
    [Header("External fields")]
    [SerializeField] private GameObject player;
    [SerializeField] private BoxCollider2D sceneBoundObj;
    [SerializeField] private CameraZones _zones;

    [Header("Offsets for camera")]
    [SerializeField] private float _lateralOffset;
    [SerializeField] private float neutralVerticalOffset;
    [SerializeField] private float _neutralLateralOffset = 4f;
    [SerializeField] private float peekUpDistance = 2.5f;
    [SerializeField] private float peekDownDistance = 3.5f;

    [Header("Camera transition speeds")]
    [SerializeField] private float _movementTransitionSpeed = 0.125f;
    [SerializeField] private float _directionalSmoothTime = 0.15f;
    [SerializeField] private float _verticalSmoothTime = 0.12f;
    [SerializeField] private float _cinematicSmoothTimeX = 0.45f; // new parameter

    // velocity holders for SmoothDamp (must be unique per SmoothDamp call)
    private float _lateralOffsetVelocity;
    private float _desiredXVelocity;
    private float _cameraVerticalVelocity;
    private float orthographicTransitionRef = 1f;

    private float _desiredX;
    private float _desiredY;
    private float _desiredZ = -12f;

    private float _forwardFacingOffset;
    private float _backwardFacingOffset;

    private Camera _mainCamera;
    private float _verticalExtent;
    private float cameraHalfWidth;
    private float _upperCameraBound;
    private float _lowerCameraBound;

    [SerializeField] private float _upperThreshold = 1.5f;
    private float _lowerThreshold;

    private Bounds sceneBounds;
    private float initOrthographicSize;
    private float inputY;

    private float lockedOffsetX;
    private bool offsetXLocked = false;

    private bool isReturningFromPeek;
    private float returnTargetY;
    private float _verticalOffset;

    private float XSmoothTimeToUse;

    public float CameraClampedX { get; private set; }
    public float CameraClampedY { get; private set; }

    private GameObject playerObject;
    private Transform _playerTransform;
    private Rigidbody2D _playerRb;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerRespawned += OnPlayerRespawned;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerRespawned -= OnPlayerRespawned;
    }

    void OnPlayerRespawned(GameObject newPlayer)
    {
        // refresh all dependent refs when the player is respawned
        playerObject = newPlayer;
        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
            _playerRb = playerObject.GetComponent<Rigidbody2D>();
            // snap camera to player nicely to avoid visible pop
            transform.position = new Vector3(
                _playerTransform.position.x + _lateralOffset,
                _playerTransform.position.y + _verticalOffset,
                _desiredZ
            );

            // recompute bounds now that camera moved
            if (_mainCamera != null)
            {
                _verticalExtent = _mainCamera.orthographicSize;
                cameraHalfWidth = _verticalExtent * _mainCamera.aspect;
                GetVerticalCameraBounds();
            }
        }
    }

    private void Awake()
    {
        playerObject = player;
    }

    void Start()
    {
        // require those objects
        if (sceneBoundObj == null) Debug.LogWarning("Scene bounds object not assigned in CameraController.");
        _mainCamera = GetComponent<Camera>();
        if (_mainCamera == null) _mainCamera = Camera.main;

        // assign player refs if a player was placed in inspector
        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
            _playerRb = playerObject.GetComponent<Rigidbody2D>();
        }

        _lateralOffset = _neutralLateralOffset;
        _verticalOffset = neutralVerticalOffset;

        _forwardFacingOffset = Mathf.Abs(_lateralOffset);
        _backwardFacingOffset = -_forwardFacingOffset;

        initOrthographicSize = _mainCamera != null ? _mainCamera.orthographicSize : 5f;
        _verticalExtent = initOrthographicSize;
        cameraHalfWidth = _verticalExtent * _mainCamera.aspect;

        GetVerticalCameraBounds();

        // compute lower threshold based on current values
        if (_playerTransform != null)
            _lowerThreshold = (_playerTransform.position.y + _verticalOffset) - _lowerCameraBound;

        if (sceneBoundObj != null)
            sceneBounds = sceneBoundObj.bounds;

        // initial placement if we have a player
        if (_playerTransform != null)
        {
            transform.position = new Vector3(
                _playerTransform.position.x + _lateralOffset,
                _playerTransform.position.y + _verticalOffset,
                _desiredZ
            );
        }
    }

    private void Update()
    {
        // if player doesn't exist yet, do nothing; wait for respawn
        if (_playerTransform == null) return;

        inputY = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
        {
            // set target as the current player's y plus neutral (use current position + current offset)
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
        // safety: wait until player exists and camera exists
        if (_playerTransform == null || _mainCamera == null) return;

        // manage orthographic size (safe SmoothDamp using dedicated ref)
        if (_zones != null && _zones.ZoneActive && _zones.CustomOrthographicSize > 0)
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

        // recompute extents and half width if size changed
        _verticalExtent = _mainCamera.orthographicSize;
        cameraHalfWidth = _verticalExtent * _mainCamera.aspect;

        // lateral logic
        DetermineLateralOffset();

        // desiredX: smooth from current desiredX towards target (use its own velocity ref)
        float targetX = (_zones != null && _zones.ZoneActive) ? lockedOffsetX : (_playerTransform.position.x + _lateralOffset);
        _desiredX = Mathf.SmoothDamp(_desiredX, targetX, ref _desiredXVelocity, XSmoothTimeToUse);

        // vertical
        GetVerticalCameraBounds();
        _desiredY = GetCameraDesiredY();

        //Debug.Log($"RETURNED DESIREDy FROM THE METHOD: {_desiredY}");

        // Combine and clamp BEFORE applying smoothing to avoid NaN
        Vector3 desiredPosition = new Vector3(_desiredX, _desiredY, _desiredZ);

        if (sceneBoundObj != null)
        {
            CameraClampedX = Mathf.Clamp(desiredPosition.x, sceneBounds.min.x + cameraHalfWidth, sceneBounds.max.x - cameraHalfWidth);
            CameraClampedY = Mathf.Clamp(desiredPosition.y, sceneBounds.min.y + _verticalExtent, sceneBounds.max.y - _verticalExtent);
            desiredPosition = new Vector3(CameraClampedX, CameraClampedY, _desiredZ);
        }

        // Smooth follow (lerp is fine here)
        // but ensure we never pass NaN into transform.position
        if (float.IsNaN(desiredPosition.x) || float.IsNaN(desiredPosition.y))
        {
            // bail out if we somehow have invalid values
            Debug.LogWarning("CameraController: desiredPosition contains NaN — skipping this frame.");
            return;
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _movementTransitionSpeed);
        transform.position = smoothedPosition;
    }

    void DetermineLateralOffset()
    {
        if (_playerTransform == null || _playerRb == null)
            return;

        float facingAngleY = _playerTransform.eulerAngles.y;
        bool isFacingLeft = Mathf.Approximately(facingAngleY, 180f);
        bool isMoving = Mathf.Abs(_playerRb.velocity.x) > 0.1f;

        float targetOffset;

        if (_zones != null && _zones.ZoneActive && _zones.XOffset != 0f)
        {
            if (!offsetXLocked)
            {
                // lock to world X position of player + zone offset
                lockedOffsetX = _playerTransform.position.x + _zones.XOffset;
                offsetXLocked = true;
            }

            targetOffset = _zones.XOffset;
            XSmoothTimeToUse = _cinematicSmoothTimeX; // slower for cinematic zones
        }
        else
        {
            offsetXLocked = false;
            targetOffset = isMoving
                ? (isFacingLeft ? -_forwardFacingOffset : _forwardFacingOffset)
                : _neutralLateralOffset;

            XSmoothTimeToUse = _directionalSmoothTime; // faster for gameplay
        }

        _lateralOffset = Mathf.SmoothDamp(
            _lateralOffset,
            targetOffset,
            ref _lateralOffsetVelocity,
            XSmoothTimeToUse
        );
    }


    void GetVerticalCameraBounds()
    {
        _upperCameraBound = transform.position.y + _verticalExtent;
        _lowerCameraBound = transform.position.y - _verticalExtent;
    }

    float GetCameraDesiredY()
    {
        float currentCamY = transform.position.y;
        float playerY = _playerTransform.position.y;

        float playerSign = playerY < 0 ? -1 : 1;

        // TODO: remove; it is needless; too many variables for the same thing
        float baseOffset = neutralVerticalOffset * playerSign;

        // Cinematic override (disable peeking in zones)
        bool allowPeek = true;
        if (_zones != null && _zones.ZoneActive)
        {
            baseOffset = _zones.YOffset * playerSign;
            allowPeek = false;
        }
        // Apply the offset to the player's Y
        float targetY = playerY + baseOffset;

        // Handle peek input (only if allowed)
        if (allowPeek)
        {
            if (Mathf.Abs(inputY) > 0.01f)
            if (inputY > 0.01f)
            {
                targetY = playerY + baseOffset + peekUpDistance * playerSign;
            }
            else if (inputY < -0.01f)
            {
                targetY = playerY + baseOffset - peekDownDistance * playerSign;
            }
        }

        // Smoothly move the camera toward target Y
        float smoothedY = Mathf.SmoothDamp(
            currentCamY,
            targetY,
            ref _cameraVerticalVelocity,
            _verticalSmoothTime
        );

        return smoothedY;
    }
}