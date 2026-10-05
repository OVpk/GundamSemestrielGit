using UnityEngine;

[CreateAssetMenu(menuName = "Data/Mecha/Set")]

public class SetData : ScriptableObject
{
    [field: SerializeField] public ModuleSet Set { get; private set; }
    
    [field: SerializeField] public ConditionnalSubstat[] SetBonus { get; private set; }
}
