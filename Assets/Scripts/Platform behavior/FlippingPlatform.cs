using System.Collections;
using UnityEngine;

public class FlippingPlatform : MonoBehaviour, IActivatable
{
    [Header("Timing")]
    [SerializeField] private bool startFrozen = false;
    [SerializeField] private bool startWithPause = false;
    [SerializeField] private float startPauseTime = 0.5f;
    [SerializeField] private float pauseBetweenFlips = 1f;

    [Header("Flip")]
    [SerializeField] private float flipAngle = 180f; // always 180 in practice

    [Header("Motion")]
    [SerializeField] private float rotationSpeed = 180f; // degrees per second

    // runtime
    private Rigidbody2D rb;
    private Coroutine pauseCoroutine;
    private float currentAngle;   // current local z-angle
    private float targetAngle;    // desired next angle
    private bool isPaused = false;
    private bool isRotating = false;
    private const float EPS = 0.05f;

    private void OnValidate()
    {
        if (rotationSpeed < 1f) rotationSpeed = 1f;
        if (pauseBetweenFlips < 0f) pauseBetweenFlips = 0f;
        if (startPauseTime < 0f) startPauseTime = 0f;
        if (flipAngle != 180f) flipAngle = 180f; // lock to 180
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

        currentAngle = transform.localEulerAngles.z;
        targetAngle = currentAngle;

        // If frozen, stay paused until Activate() handles it.
        if (startFrozen)
        {
            isPaused = true;
            return;
        }

        InitializeFlippingRoutine();
    }

    private void InitializeFlippingRoutine()
    {
        if (startWithPause)
        {
            isPaused = true;
            StartCoroutine(StartDelayCoroutine());
        }
        else
        {
            isPaused = false;
        }
    }


    private IEnumerator StartDelayCoroutine()
    {
        yield return new WaitForSeconds(startPauseTime);
        isPaused = false;
    }

    private void FixedUpdate()
    {
        if (isPaused) return;

        // prepare next flip
        if (!isRotating)
        {
            targetAngle = currentAngle + flipAngle;
            isRotating = true;
        }

        // smooth rotation toward target
        if (isRotating)
        {
            currentAngle = Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                rotationSpeed * Time.fixedDeltaTime
            );

            rb.MoveRotation(currentAngle);

            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle)) <= EPS)
            {
                currentAngle = targetAngle;
                rb.MoveRotation(currentAngle);
                isRotating = false;

                if (pauseCoroutine == null)
                    pauseCoroutine = StartCoroutine(PauseAfterFlipCoroutine());
            }
        }
    }

    private IEnumerator PauseAfterFlipCoroutine()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseBetweenFlips);
        isPaused = false;
        pauseCoroutine = null;
    }

    private void OnDisable()
    {
        if (pauseCoroutine != null) StopCoroutine(pauseCoroutine);
    }

    public void Activate()
    {
        if (!isPaused) // If already running, ignore reactivation
            return;

        // Unfreeze
        startFrozen = false;
        InitializeFlippingRoutine();
    }
}



