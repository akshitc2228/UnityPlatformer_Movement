using UnityEngine;

public enum MovementDirection { Sideways, Vertical }

public abstract class PlatformMover : MonoBehaviour
{
    [Header("Platform movement Settings")]
    [SerializeField] protected MovementDirection direction;

    [Header("Waypoints")]
    [SerializeField] protected Transform boundOne;
    [SerializeField] protected Transform boundTwo;
    //waypoints are attached as children so if you want them to follow the parent for some reason
    [SerializeField] protected bool followLiveWaypoints = false;

    protected Vector3 startPos;
    protected Vector3 targetPos;
      
    protected Vector3 cachedOne;
    protected Vector3 cachedTwo;

    [SerializeField] protected bool startMoveToUpper = true;

    protected virtual void Start()
    {
        // Validate bounds
        if (!boundOne || !boundTwo)
        {
            Debug.LogError($"[{name}] PlatformMover requires both boundOne and boundTwo assigned!");
            enabled = false;
            return;
        }

        // Cache world positions if not following live transforms
        if (!followLiveWaypoints)
        {
            cachedOne = boundOne.position;
            cachedTwo = boundTwo.position;
        }

        // Start heading toward upper bound first
        if(!followLiveWaypoints)
        {
            targetPos = startMoveToUpper ? cachedOne : cachedTwo;
        }
        else
        {
            targetPos = startMoveToUpper ? boundOne.position : boundTwo.position;
        }
    }

    protected virtual void Update()
    {
        float movementSpeed = DefineSpeed();
        transform.position = Vector3.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);

        // If we reached target, flip direction
        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            OnReachedTarget();
        }
    }

    protected abstract float DefineSpeed();

    // Subclasses can override how/when direction flips
    protected virtual void OnReachedTarget()
    {
        startMoveToUpper = !startMoveToUpper;
        Vector3 currentOne = followLiveWaypoints ? boundOne.position : cachedOne;
        Vector3 currentTwo = followLiveWaypoints ? boundTwo.position : cachedTwo;
        targetPos = startMoveToUpper ? currentOne : currentTwo;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Vector2 drawDirection = direction == MovementDirection.Sideways ? Vector2.up : Vector2.right;

        Gizmos.DrawRay(boundOne.position, drawDirection * 4f);
        Gizmos.DrawRay(boundTwo.position, drawDirection * 4f);
    }
}

