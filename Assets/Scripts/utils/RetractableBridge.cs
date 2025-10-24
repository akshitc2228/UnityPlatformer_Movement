using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetractableBridge : MonoBehaviour, IActivatable
{
    [SerializeField] private Transform bridgeTargetTransform;
    [SerializeField] private float bridgeMovementSpeed = 2f;
    [SerializeField] private bool moveUpwards = false;

    private bool isMoving = false;

    public void Activate()
    {
        if (!isMoving)
            StartCoroutine(MoveBridge());
    }

    private IEnumerator MoveBridge()
    {
        isMoving = true;

        Vector3 endPos = new Vector3(
            !moveUpwards ? bridgeTargetTransform.position.x : transform.position.x,
            !moveUpwards ? transform.position.y : bridgeTargetTransform.position.y,
            transform.position.z
        );

        while (Vector3.Distance(transform.position, endPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                endPos,
                bridgeMovementSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = endPos;
        isMoving = false;
    }
}

