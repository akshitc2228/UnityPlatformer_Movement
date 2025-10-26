using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    //only need for debugging
    [SerializeField] private string checkpointName;
    private Transform spawnPoint;

    private void Awake()
    {
        checkpointName = this.gameObject.name;
        if (transform.childCount == 0)
            throw new MissingReferenceException($"Checkpoint '{name}' has no spawn point attached.");

        spawnPoint = transform.GetChild(0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.SetCheckpoint(spawnPoint);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().bounds.size);
    }
}

