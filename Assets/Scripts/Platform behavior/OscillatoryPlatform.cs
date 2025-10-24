using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillatoryPlatform : PlatformMover, IActivatable
{
    [SerializeField] private bool startFrozen;
    [SerializeField] private float oscillationSpeed;

    private HazardDamage damageScript;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        if(startFrozen)
        {
            damageScript = GetComponent<HazardDamage>();
            animator = GetComponent<Animator>();

            if(damageScript != null && animator != null)
            {
                damageScript.DisableDamage = true;
                animator.enabled = false;
            }
            else
            {
                throw new UnassignedReferenceException();
            }
        }
    }

    protected override float DefineSpeed()
    {
        if (startFrozen) return 0f;
        return oscillationSpeed;
    }

    public void Activate()
    {
        if(startFrozen)
        {
            startFrozen = false;
            damageScript.DisableDamage = false;
            animator.enabled = true;
        }
    }
}
