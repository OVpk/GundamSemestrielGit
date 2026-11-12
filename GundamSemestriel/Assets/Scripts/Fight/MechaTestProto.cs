using UnityEngine;

public class MechaTestProto : MonoBehaviour
{
    [SerializeField] private MechaBaseData baseData;
    [SerializeField] private ModuleData[] modules = new ModuleData[5];
    
    public MechaBaseData BaseData => baseData;
    public ModuleData[] Modules => modules;

    public void DisplayCalculatedStats()
    {
        var runtimeData = new EntityRuntimeData(BaseData, Modules);
        
        Debug.Log($"Stats finales en jeu :\n" +
                  $"HP : {runtimeData.Hp}\n" +
                  $"Attack : {runtimeData.Attack}\n" +
                  $"Def : {runtimeData.Def}\n" +
                  $"Speed : {runtimeData.Speed}\n" +
                  $"CritRate : {runtimeData.CritRate}\n" +
                  $"CritDamage : {runtimeData.CritDamage}");
    }
}