using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiFunctionalCrank : SimpleCrank
{
    [SerializeField] private List<MonoBehaviour> targets = new List<MonoBehaviour>();

    protected override void ActivationAction()
    {
        foreach (var target in targets)
        {
            if (target is IActivatable activatable)
                activatable.Activate();
        }
    }
}

