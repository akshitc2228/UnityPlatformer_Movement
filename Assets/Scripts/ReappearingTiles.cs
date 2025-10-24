using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ReappearingTiles : MonoBehaviour
{
    [SerializeField] private float blinkDuration = 0.2f;
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float interval = 2f;

    private TilemapRenderer tRenderer;
    private TilemapCollider2D tCollider;
    private Coroutine blinkCoroutine;

    private bool active = true;

    private void Start()
    {
        tRenderer = GetComponent<TilemapRenderer>();
        tCollider = GetComponent<TilemapCollider2D>();
        StartCoroutine(CycleHazard());
    }

    private IEnumerator CycleHazard()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            yield return StartCoroutine(BlinkWarning());
            SetActive(false);

            yield return new WaitForSeconds(interval);

            yield return StartCoroutine(BlinkWarning());
            SetActive(true);
        }
    }

    private void SetActive(bool state)
    {
        active = state;
        if (tRenderer != null) tRenderer.enabled = state;
        if (tCollider != null) tCollider.enabled = state;
    }

    private IEnumerator BlinkWarning()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            if (tRenderer != null) tRenderer.enabled = false;
            yield return new WaitForSeconds(blinkDuration);

            if (tRenderer != null) tRenderer.enabled = true;
            yield return new WaitForSeconds(blinkDuration);
        }

        blinkCoroutine = null;
    }
}

