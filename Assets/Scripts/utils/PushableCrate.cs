using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushableCrate : MonoBehaviour
{
    public float Weight { get; private set; } = 5f;
    public bool OnALift { get; set; }

    [Header ("pushing parameters")]
    [SerializeField] private float pushSpeedModifier = 1f;
    [SerializeField] private float cratePushSmoothTime = 0.8f;

    [Header("Externals")]
    [SerializeField] private CrateLift crateLift;

    private Rigidbody2D crateRb;
    private float crateVelRef = 0f;

    private void Awake()
    {
        crateRb = GetComponent<Rigidbody2D>();
        if(crateLift == null)
        {
            throw new MissingReferenceException();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            //impart player velocity to this crate with some modifier
            float newXVel = Mathf.SmoothDamp(
                    crateRb.velocity.x,
                    collision.otherRigidbody.velocity.x * pushSpeedModifier,
                    ref crateVelRef,
                    cratePushSmoothTime
            );
            crateRb.velocity = new Vector2(newXVel, crateRb.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if its another box, we update the global weight list for the lift
        if (collision.gameObject.CompareTag("dungeon2_pushableCrate"))
        {
            if (!collision.gameObject.GetComponent<PushableCrate>().OnALift) return;
            crateLift.NetLiftWeight += Weight;
            OnALift = true;
            crateLift.CheckAndUpdateMapIndex();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            //remove all velocity from this box:
            crateRb.velocity = Vector2.zero;
        }

        if (collision.gameObject.CompareTag("dungeon2_pushableCrate"))
        {
            if (!collision.gameObject.GetComponent<PushableCrate>().OnALift) return;
            crateLift.NetLiftWeight -= Weight;
            OnALift = false;
            crateLift.CheckAndUpdateMapIndex();
        }
    }
}
