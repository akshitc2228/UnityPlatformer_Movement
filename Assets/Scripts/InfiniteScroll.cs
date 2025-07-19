using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParallaxBehavior))]
public class InfiniteScroll : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;

    private ParallaxBehavior parallaxLayer;
    private float scrollThresholdWidth;
    private float layerLeftEdge;
    private float layerRightEdge;

    private float oneSpriteBound = 0f;

    void Awake()
    {
        parallaxLayer = GetComponent<ParallaxBehavior>();
        scrollThresholdWidth = ComputeVisualWidthFromChildren();
    }

    void Start()
    {
        Vector3 startAnchor = parallaxLayer.GetAnchor();
        layerLeftEdge = startAnchor.x - scrollThresholdWidth / 2;
        layerRightEdge = startAnchor.x + scrollThresholdWidth / 2;
    }

    void Update()
    {
        Vector3 camPos = playerCamera.position;
        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = camHalfHeight * Camera.main.aspect;

        float camLeft = camPos.x - camHalfWidth;
        float camRight = camPos.x + camHalfWidth;

        if (camRight > layerRightEdge)
        {
            if(oneSpriteBound > 0f)
            {
                ShiftLayer(oneSpriteBound);
            }
        }
        else if (camLeft < layerLeftEdge)
        {
            if (oneSpriteBound > 0f)
            {
                ShiftLayer(-oneSpriteBound);
            }
        }
    }

    void ShiftLayer(float amount)
    {
        parallaxLayer.SetAnchor(parallaxLayer.GetAnchor() + new Vector3(amount, 0, 0));
        layerLeftEdge += amount;
        layerRightEdge += amount;
    }

    private float ComputeVisualWidthFromChildren()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        if (sprites.Length == 0) return 0f;

        oneSpriteBound = sprites[0].bounds.size.x;

        Bounds combined = sprites[0].bounds;
        foreach (var sr in sprites)
            combined.Encapsulate(sr.bounds);

        return combined.size.x;
    }

    private void OnDrawGizmos()
    {
        Vector3 camPos = playerCamera.position;
        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = camHalfHeight * Camera.main.aspect;

        float camLeft = camPos.x - camHalfWidth;
        float camRight = camPos.x + camHalfWidth;
        Gizmos.color = Color.black;
        Gizmos.DrawRay(new Vector3(camLeft, 0, 0), Vector2.up * 5f);
        Gizmos.DrawRay(new Vector3(camRight, 0, 0), Vector2.up * 5f);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(new Vector3(layerLeftEdge, 0, 0), Vector2.up * 10f);
        Gizmos.DrawRay(new Vector3(layerRightEdge, 0, 0), Vector2.up * 10f);
    }
}

