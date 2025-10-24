using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableCrate : MonoBehaviour
{
    [SerializeField] private float pushSpeedModifier = 1f;
    [SerializeField] private float cratePushSmoothTime = 0.8f;
    private Rigidbody2D crateRb;
    private float crateVelRef = 0f;

    private void Awake()
    {
        crateRb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            //impart player velocity to this crate with some modifier
            float newXVel = Mathf.SmoothDamp(
                    crateRb.velocity.x,
                    collision.otherRigidbody.velocity.x,
                    ref crateVelRef,
                    cratePushSmoothTime
            );
            crateRb.velocity = new Vector2(newXVel, crateRb.velocity.y);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            //remove all velocity from this box:
            crateRb.velocity = Vector2.zero;
        }
    }
}
