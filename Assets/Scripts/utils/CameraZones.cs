using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZones : MonoBehaviour
{
    public float XOffset {  get; private set; }
    public float YOffset {  get; private set; }
    public float CustomOrthographicSize { get; private set; }
    public bool ZoneActive { get; private set; }


    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider != null && collider.CompareTag("FlippingRange_1"))
        {
            //first check the player zone:
            Transform playerTransform = this.transform;
            float facingAngleY = playerTransform.eulerAngles.y;
            bool isFacingLeft = Mathf.Approximately(facingAngleY, 180f);

            ZoneActive = true;
            XOffset = !isFacingLeft ? 17 : -17;
            CustomOrthographicSize = 13;
        }

        if (collider != null && collider.CompareTag("SlopeAndKhai"))
        {
            //first check the player zone:
            Transform playerTransform = this.transform;
            float facingAngleY = playerTransform.eulerAngles.y;
            bool isFacingLeft = Mathf.Approximately(facingAngleY, 180f);

            ZoneActive = true;
            XOffset = !isFacingLeft ? 20 : -20;
            CustomOrthographicSize = 20;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision != null && collision.CompareTag("FlippingRange_1"))
        {
            ZoneActive = false;
        }

        if (collision != null && collision.CompareTag("SlopeAndKhai"))
        {
            ZoneActive = false;
        }
    }
}
