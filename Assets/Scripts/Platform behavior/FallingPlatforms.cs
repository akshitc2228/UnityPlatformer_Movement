using System.Collections;
using UnityEngine;

public class FallingPlatforms : MonoBehaviour
{
    private Rigidbody2D rb;
    private Coroutine judderCoroutine;

    [SerializeField] private float judderDuration = 4.5f;
    [SerializeField] private float judderIntensity = 0.05f;
    [SerializeField] private Transform spriteHolder;
    [SerializeField] private SceneBoundsSO sceneBounds;

    private Vector3 originalPosition;
    private bool hasFallen = false;
    private bool isJuddering = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
    }

    private void Update()
    {
        if (transform.position.y <= sceneBounds.minY)
            Destroy(gameObject);
    }

    private IEnumerator JudderWindow()
    {
        rb.velocity = Vector2.zero;
        isJuddering = true;
        float elapsed = 0f;
        while (elapsed < judderDuration)
        {
            float offsetX = Random.Range(-judderIntensity, judderIntensity);
            float offsetY = Random.Range(-judderIntensity, judderIntensity);
            spriteHolder.localPosition = new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteHolder.localPosition = Vector3.zero;

        // unparent any children (e.g. player)
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Player"))
                child.SetParent(null);
        }

        rb.velocity = new Vector2(rb.velocity.x, -Physics2D.gravity.magnitude * 1.5f);
        hasFallen = true;
        isJuddering = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasFallen) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            if (judderCoroutine == null)
            {
                collision.gameObject.transform.SetParent(this.gameObject.transform);
                judderCoroutine = StartCoroutine(JudderWindow());
            }
        }
    }

}

