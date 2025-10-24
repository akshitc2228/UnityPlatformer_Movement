using UnityEngine;

[ExecuteInEditMode] // also works in editor
public class CenterGameObject : MonoBehaviour
{
    [ContextMenu("Center Pivot On Children")]
    private void CenterPivotNow()
    {
        if (transform.childCount == 0) return;

        // calculate average center of all child renderers
        Bounds bounds = new Bounds(transform.GetChild(0).position, Vector3.zero);
        foreach (Transform child in transform)
        {
            Renderer r = child.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                bounds.Encapsulate(r.bounds);
            }
            else
            {
                bounds.Encapsulate(child.position);
            }
        }

        Vector3 center = bounds.center;

        // move parent to center
        Vector3 offset = transform.position - center;
        transform.position = center;

        // apply offset back to children so visuals don’t move
        foreach (Transform child in transform)
        {
            child.position += offset;
        }
    }
}

