using UnityEngine;

public class MechaTestProto : MonoBehaviour
{
    [SerializeField] private MechaBaseData baseData;
    
    [Header("Modules")]
    [SerializeField] private ModuleData headModule;
    [SerializeField] private ModuleData leftArmModule;
    [SerializeField] private ModuleData rightArmModule;
    [SerializeField] private ModuleData leftLegModule;
    [SerializeField] private ModuleData rightLegModule;

    public MechaBaseData BaseData => baseData;
    
    public ModuleData[] Modules => new ModuleData[] 
    { 
        headModule, 
        leftArmModule, 
        rightArmModule, 
        leftLegModule, 
        rightLegModule 
    };

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