using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Freeze Player Input")]
public class FreezeInputEventSO : ScriptableObject
{
    public event Action<bool> FreezePlayerInput;
    public void Raise(bool freeze) => FreezePlayerInput?.Invoke(freeze);
}
