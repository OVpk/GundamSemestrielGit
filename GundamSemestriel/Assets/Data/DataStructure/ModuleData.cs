using UnityEngine;

[CreateAssetMenu(menuName = "Data/Mecha/Module")]
public class ModuleData : ScriptableObject
{
    [SerializeField] private SetData set;
    
    public SetData Set => set;
    
    [field: SerializeField] public SubStat[] SubStats { get; private set; }
}
