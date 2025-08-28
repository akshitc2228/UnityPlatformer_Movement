using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsProfile", menuName = "Settings/Physics Profile")]
public class PhysicsProfile : ScriptableObject
{
    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public LayerMask hybridLayer;

    public float GlobalGravityScaleReference { get; set; }
}