using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBehavior : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField, Range(0f, 1f)] private float parallaxFactorX = 0.5f;
    [SerializeField, Range(0f, 1f)] private float parallaxFactorY = 0f;

    private Vector3 camStartPos;
    private Vector3 layerAnchorPos;

    public void SetAnchor(Vector3 newAnchor) => layerAnchorPos = newAnchor;
    public Vector3 GetAnchor() => layerAnchorPos;

    void Start()
    {
        camStartPos = playerCamera.position;
        layerAnchorPos = transform.position;
    }

    void LateUpdate()
    {
        Vector3 camDelta = playerCamera.position - camStartPos;
        float offsetX = camDelta.x * parallaxFactorX;
        float offsetY = camDelta.y * parallaxFactorY;

        transform.position = new Vector3(
            layerAnchorPos.x + offsetX,
            layerAnchorPos.y + offsetY,
            transform.position.z
        );
    }
}


