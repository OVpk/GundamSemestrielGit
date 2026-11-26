using UnityEngine;

[CreateAssetMenu(menuName = "Data/Mecha/Base")]
public class MechaBaseData : EntityBaseData
{
    public GameObject prefab;
    
    [field: SerializeField] public SetData Set { get; private set; }
}
