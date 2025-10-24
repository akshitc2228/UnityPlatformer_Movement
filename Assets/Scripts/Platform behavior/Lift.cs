using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Lift : PlatformMover, IActivatable
{
    [SerializeField] private float liftSpeed = 3f;
    [SerializeField] private GameObject leftBound;
    [SerializeField] private GameObject rightBound;

    private bool isMoving = false;
    private bool reachedStop = false;
    private bool playerOnLift = false;

    private void Awake()
    {
        if (leftBound) leftBound.SetActive(false);
        if (rightBound) rightBound.SetActive(false);

        startPos = transform.position;
    }

    protected override float DefineSpeed()
    {
        if (!isMoving || reachedStop) return 0f;
        return liftSpeed;
    }

    public void Activate()
    {
        if (!isMoving)
            StartCoroutine(MoveLift());
    }

    private IEnumerator MoveLift()
    {
        isMoving = true;
        reachedStop = false;

        if (leftBound) leftBound.SetActive(true);
        if (rightBound) rightBound.SetActive(true);

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            yield return null;
        }

        // stop moving completely
        isMoving = false;
        reachedStop = true;

        if (leftBound) leftBound.SetActive(false);
        if (rightBound) rightBound.SetActive(false);
    }

    protected override void OnReachedTarget()
    {
        // The lift has reached its destination; stop here
        Debug.Log("Lift reached target point");
        isMoving = false;
        reachedStop = true;

        // DO NOT flip direction here — handled when player exits
    }

    public void OnPlayerEntered()
    {
        if (!playerOnLift && !isMoving && !reachedStop)
        {
            playerOnLift = true;
            Activate();
        }
    }

    public void OnPlayerExited()
    {
        playerOnLift = false;

        if (reachedStop)
        {
            // Flip direction for the next ride
            startMoveToUpper = !startMoveToUpper;
            Vector3 currentOne = followLiveWaypoints ? boundOne.position : cachedOne;
            Vector3 currentTwo = followLiveWaypoints ? boundTwo.position : cachedTwo;
            targetPos = startMoveToUpper ? currentOne : currentTwo;

            reachedStop = false; // ready to move again next time
        }
    }
}

