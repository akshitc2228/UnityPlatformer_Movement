using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlMenuModal : MonoBehaviour
{
    [SerializeField] protected GameObject controlledPanel;

    protected virtual void Awake()
    {
        if (controlledPanel != null)
            controlledPanel.SetActive(false);
    }

    public virtual void ShowModal()
    {
        if (controlledPanel != null)
            controlledPanel.SetActive(true);
    }

    public virtual void HideModal()
    {
        if (controlledPanel != null)
            controlledPanel.SetActive(false);
    }
}

