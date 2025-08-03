using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlMenuModal : MonoBehaviour
{
    [SerializeField]
    private GameObject OptionsPanel;

    private void Awake() => OptionsPanel.SetActive(false);

    public void ShowModal() => OptionsPanel.SetActive(true);
    public void HideModal() => OptionsPanel.SetActive(false);
}
