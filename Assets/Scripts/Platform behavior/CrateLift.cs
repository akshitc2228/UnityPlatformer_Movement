using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CrateLift : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<Transform> targetPoints;
    [SerializeField] private float liftMoveTime = 0.9f;

    [SerializeField] private List<MonoBehaviour> affected = new List<MonoBehaviour>();

    private Vector3 velocity = Vector3.zero;

    private List<KeyValuePair<Transform, float>> orderedThresholds;
    private int globalMapIndex;
    private int localUpdatedIndex;
    public float NetLiftWeight { get; set; }

    private void Start()
    {
        var thresholds = new Dictionary<Transform, float>();
        float threshold = 0f;
        thresholds.Add(transform, threshold);
        foreach (var t in targetPoints)
        {
            threshold += 5f;
            thresholds.Add(t, threshold);
        }

        orderedThresholds = thresholds.OrderBy(x => x.Value).ToList();
        globalMapIndex = 0;
        localUpdatedIndex = 0;
    }

    private void FixedUpdate()
    {
        if (orderedThresholds.Count == 0) return;
        localUpdatedIndex = globalMapIndex;
        Vector3 targetPos = orderedThresholds[localUpdatedIndex].Key.position;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            liftMoveTime
        );

        if(localUpdatedIndex == orderedThresholds.Count - 1)
        {
            foreach (var target in affected)
            {
                if (target is IActivatable activatable)
                    activatable.Activate();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("dungeon2_pushableCrate"))
        {
            //check that its a crate before adding its weight else add nothing:
            PushableCrate collidingCrate = collision.gameObject.GetComponent<PushableCrate>();
            if (collidingCrate != null)
            {
                //also, the crate is now linked to the lift so weights can be stacked:
                collidingCrate.OnALift = true;
                NetLiftWeight += collidingCrate.Weight;
                CheckAndUpdateMapIndex();
            }
        }

        collision.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("dungeon2_pushableCrate"))
        {
            PushableCrate collidingCrate = collision.gameObject.GetComponent<PushableCrate>();
            if (collidingCrate != null)
            {
                collidingCrate.OnALift = false;
                NetLiftWeight -= collidingCrate.Weight;
                CheckAndUpdateMapIndex();
            }
        }

        collision.transform.SetParent(null);
    }

    //ideally I would have wanted to call this method
    //the same way people use a useEffect in React whenever weight changes
    //for now all I know is to make it public
    public void CheckAndUpdateMapIndex()
    {
        int newIndex = 0;
        for (int i = 0; i < orderedThresholds.Count; i++)
        {
            if (NetLiftWeight >= orderedThresholds[i].Value)
                newIndex = i;
            else
                break;
        }
        globalMapIndex = newIndex;
    }
}

