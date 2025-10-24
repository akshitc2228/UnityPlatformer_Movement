using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftBaseDetector : MonoBehaviour
{
    public Lift lift;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            lift.OnPlayerEntered();
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            lift.OnPlayerExited();
            DetachAllChildren();
        }
    }

    private void DetachAllChildren()
    {
        // remove any leftover passengers
        foreach (Transform child in transform)
        {
            child.SetParent(null);
        }
    }

}
