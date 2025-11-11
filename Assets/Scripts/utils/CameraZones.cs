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
        if(collider != null)
        {
            if(collider.CompareTag("FlippingRange_1"))
            {
                Transform playerTransform = this.transform;
                float facingAngleY = playerTransform.eulerAngles.y;
                bool isFacingLeft = Mathf.Approximately(facingAngleY, 180f);

                ZoneActive = true;
                XOffset = !isFacingLeft ? 17 : -17;
                CustomOrthographicSize = 13;
            }

            if (collider.CompareTag("SlopeAndKhai"))
            {
                Transform playerTransform = this.transform;
                float facingAngleY = playerTransform.eulerAngles.y;
                bool isFacingLeft = Mathf.Approximately(facingAngleY, 180f);

                ZoneActive = true;
                XOffset = !isFacingLeft ? 20 : -20;
                CustomOrthographicSize = 20;
            }

            if (collider.CompareTag("HiddenDungeon"))
            {
                Transform playerTransform = this.transform;
                float facingAngleY = playerTransform.eulerAngles.y;
                bool isFacingLeft = Mathf.Approximately(facingAngleY, 180f);

                ZoneActive = true;
                CustomOrthographicSize = 11f;
                YOffset = -5;
                XOffset = !isFacingLeft ? 12 : -12;
            }

            if (collider.gameObject.name.StartsWith("DisappearingSpikes_"))
            {
                ZoneActive = true;
                XOffset = 0.5f;
                YOffset = 5;
                CustomOrthographicSize = 17;
            }

            if(collider.gameObject.name.StartsWith("LowerBridge"))
            {
                ZoneActive = true;
                XOffset = 0.5f;
                CustomOrthographicSize = 11.5f;
            }            
            
            if(collider.gameObject.name.StartsWith("Large_parkour_area"))
            {
                ZoneActive = true;
                XOffset = 0.5f;
                CustomOrthographicSize = 17f;
            }
            if (collider.gameObject.name.StartsWith("pillars_gap"))
            {
                ZoneActive = true;
                XOffset = 5f;
                CustomOrthographicSize = 15f;
            }            
            if (collider.gameObject.name.StartsWith("avant_lift"))
            {
                ZoneActive = true;
                XOffset = 0.5f;
                CustomOrthographicSize = 15f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //condense to collision exit with anything
        //if(collision != null && collision.CompareTag("FlippingRange_1"))
        //{
        //    ZoneActive = false;
        //}

        //if (collision != null && collision.CompareTag("SlopeAndKhai"))
        //{
        //    ZoneActive = false;
        //}

        //if (collision != null && collision.CompareTag("HiddenDungeon"))
        //{
        //    ZoneActive = false;
        //}

        //if (collision != null && collision.gameObject.name.StartsWith("DisappearingSpikes_"))
        //{
        //    ZoneActive = false;
        //}
        //if (collision != null && collision.gameObject.name.StartsWith("LowerBridge"))
        //    ZoneActive = false;        
        //if (collision != null && collision.gameObject.name.StartsWith("Large_parkour_area"))
        //    ZoneActive = false;
        if (collision != null)
            ZoneActive = false;
    }
}
