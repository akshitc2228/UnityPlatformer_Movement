using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class SmashingPlatforms : PlatformMover, IActivatable
{
    [SerializeField] private float descendSpeed = 8f;
    [SerializeField] private float ascendSpeed = 2f;
    [SerializeField] private float backToStartSpeed = 8f;
    [SerializeField] private float pauseTime = 0.5f;
    [SerializeField] private float initialDelayTime = 0.5f;
    [SerializeField] private bool startWithPause = false;
    [SerializeField] private float customDamage = 0f;

    [SerializeField] private PlayerHealth playerHealth;

    private bool isPaused = false;
    private Coroutine PauseCoroutine;

    private Vector3 startPosition;
    private Rigidbody2D rb;
    private bool isFrozen;
    private bool isReturningToStart = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
    }

    protected override void Start()
    {
        base.Start();
        if (startWithPause)
        {
            if (PauseCoroutine == null)
            {
                StartCoroutine(PauseBeforeAscend(initialDelayTime));
            }
        }
    }

    protected override float DefineSpeed()
    {
        if (isFrozen || isPaused) return 0f;
        if (isReturningToStart) return backToStartSpeed;
        return startMoveToUpper ? ascendSpeed : descendSpeed;
    }

    protected override void OnReachedTarget()
    {
        if(isFrozen) return;
        if (startMoveToUpper) // pause before climbing back up
        {
            if (PauseCoroutine == null)
                PauseCoroutine = StartCoroutine(PauseBeforeAscend(pauseTime));
        }

        base.OnReachedTarget();
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFrozen) return;
        if(collision.gameObject.CompareTag("Player") && playerHealth != null)
        {
            float damageAmount = customDamage > 0f ? customDamage : 75f;
            playerHealth.ReduceCurrentHealth(damageAmount);
        }
    }

    IEnumerator PauseBeforeAscend(float delayTime)
    {
        isPaused = true;
        yield return new WaitForSeconds(delayTime);
        isPaused = false;
        PauseCoroutine = null; // reset
    }

    public void Activate()
    {
        if (isFrozen) return;

        StopAllCoroutines();
        isReturningToStart = true;
        targetPos = startPosition;
        StartCoroutine(ReturnAndFreeze());
    }

    private IEnumerator ReturnAndFreeze()
    {
        // Wait until we're nearly back at start
        while (Vector3.Distance(transform.position, startPosition) > 0.05f)
            yield return null;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        isReturningToStart = false;
        isFrozen = true;
    }
}
