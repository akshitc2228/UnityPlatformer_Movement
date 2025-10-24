using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncSceneBounds : MonoBehaviour
{
    [SerializeField] private SceneBoundsSO sceneBoundsSO;

    private void OnValidate()
    {
        var bounds = GetComponent<Collider2D>().bounds;
        sceneBoundsSO.minX = bounds.min.x;
        sceneBoundsSO.maxX = bounds.max.x;
        sceneBoundsSO.minY = bounds.min.y;
        sceneBoundsSO.maxY = bounds.max.y;
    }
}
