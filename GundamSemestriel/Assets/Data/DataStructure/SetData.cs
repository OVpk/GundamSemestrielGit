using UnityEngine;

[CreateAssetMenu(menuName = "Data/Mecha/Set")]

public class SetData : ScriptableObject
{
    [field: SerializeField] private ModuleSet Set { get; set; }
    
    [field: SerializeField] private ConditionnalSubstat[] SetBonus { get; set; }
}
