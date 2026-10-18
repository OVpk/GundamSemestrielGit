using UnityEngine;

[CreateAssetMenu(menuName = "Data/Mecha/Base")]
public class MechaBaseData : EntityBaseData
{
    [field: SerializeField] public SetData Set { get; private set; }
}
