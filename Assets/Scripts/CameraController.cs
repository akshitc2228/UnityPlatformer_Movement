using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //external fields:
    [Header("External fields")]
    [SerializeField]
    private Transform _playerTransform;
    [SerializeField]
    private Rigidbody2D _playerRb;
    [SerializeField] 
    private BoxCollider2D sceneBoundObj;

    //offsets:
    [Header("Offsets for camera")]
    [SerializeField]
    private float _lateralOffset;
    [SerializeField]
    private float _verticalOffset;
    [SerializeField]
    float _neutralLateralOffset = 0.5f;

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
    float _verticalTransitionSpeed = 0.7f;
    [SerializeField]
    private float _verticalSmoothTime;

    //TODO: in a full level bounds would need to be introduced to clamp camera position at start and end of level
    //internal camera variables:
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

    private Bounds sceneBounds;

    //whether we need a setter yet is debatable
    public Bounds GetSceneBounds() => sceneBounds;

    // Start is called before the first frame update
    void Start()
    {
        _lateralOffset = _neutralLateralOffset;
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
        GetVerticalCameraBounds();
        cameraHalfWidth = _verticalExtent * Camera.main.aspect;

        //this should always be fixed
        _lowerThreshold = (_playerTransform.position.y + _verticalOffset) - _lowerCameraBound;

        //define screen bounds
        sceneBounds = sceneBoundObj.bounds;
    }

    private void LateUpdate()
    {
        //move lateral camera:
        MoveDirectionCamera();
        _desiredX = _playerTransform.position.x + _lateralOffset;

        //vertical camera movement:
        GetVerticalCameraBounds();
        _desiredY = GetCameraDesiredY();

        //Combine into desired position
        Vector3 desiredPosition = new Vector3(_desiredX, _desiredY, _desiredZ);

        //Clamp desired position BEFORE applying smoothing
        float clampedX = Mathf.Clamp(desiredPosition.x, sceneBounds.min.x + cameraHalfWidth, sceneBounds.max.x - cameraHalfWidth);
        float clampedY = Mathf.Clamp(desiredPosition.y, sceneBounds.min.y + _verticalExtent, sceneBounds.max.y - _verticalExtent);
        desiredPosition = new Vector3(clampedX, clampedY, _desiredZ);

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

        float targetOffset = isMoving
            ? (isFacingLeft ? _backwardFacingOffset : _forwardFacingOffset)
            : _neutralLateralOffset;

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
        float playerY = _playerTransform.position.y + _verticalOffset;

        float currentTopDistance = _upperCameraBound - playerY;
        float currentBottomDistance = playerY - _lowerCameraBound;

        float currentCamY = transform.position.y;

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
