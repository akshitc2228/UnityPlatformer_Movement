using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneBounds", menuName = "Game/Scene Bounds")]
public class SceneBoundsSO : ScriptableObject
{
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public Bounds ToBounds()
    {
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
        return new Bounds(center, size);
    }
}
