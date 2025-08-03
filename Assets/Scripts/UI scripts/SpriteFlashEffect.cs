using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFlashEffect : MonoBehaviour
{
    [Header("Flash material an duration")]
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration;
    [SerializeField] private float flashInterval;
    [SerializeField] private Color flashColor = Color.red;

    private Material originalMaterial;
    private SpriteRenderer currentSr;
    private Coroutine flashRoutine;

    private PlayerHealth playerHealth;

    private void OnEnable()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerHealth.OnPlayerHurt += FlashSprite;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSr = GetComponent<SpriteRenderer>();
        originalMaterial = currentSr.material;
    }

    void FlashSprite()
    {
        if (flashRoutine == null)
            StartCoroutine(FlashCoroutine());
    }

    //TODO: Replace with a shader for white flash instead
    private IEnumerator FlashCoroutine()
    {
        float elapsed = 0f;
        bool isOriginal = false;

        while (elapsed < flashDuration)
        {
            currentSr.color = isOriginal ? Color.white : flashColor;
            isOriginal = !isOriginal;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        currentSr.color = Color.white;
        flashRoutine = null;
    }


    private void OnDisable()
    {
        if(playerHealth != null)
            playerHealth.OnPlayerHurt -= FlashSprite;
    }
}
