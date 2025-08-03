using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsLayers", menuName = "Settings/Physics Layers")]
public class PhysicsLayerProfile : ScriptableObject
{
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public LayerMask hybridLayer;
}

