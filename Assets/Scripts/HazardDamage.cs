using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HazardDamage : MonoBehaviour
{
    [SerializeField] private bool killerHazard;
    [SerializeField] private float damageAmount = 25f;

    public bool DisableDamage { get; set; }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (DisableDamage) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (killerHazard) playerHealth.ReduceCurrentHealth(playerHealth.GetCurrentHealth());
                playerHealth.ReduceCurrentHealth(damageAmount);
            }
        }
    }
}
