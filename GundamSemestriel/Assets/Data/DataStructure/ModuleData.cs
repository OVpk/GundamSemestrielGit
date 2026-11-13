using UnityEngine;

[CreateAssetMenu(menuName = "Data/Mecha/Module")]
public class ModuleData : ScriptableObject
{
    public GameObject prefab;
    
    [SerializeField] private SetData set;
    
    public SetData Set => set;
    
    [field: SerializeField] public SubStat[] SubStats { get; private set; }
}
