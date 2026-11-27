using UnityEngine;

public class MechaTestProto : MonoBehaviour
{
    [Header("Visuel")]
    [SerializeField] private MechaVisualAssembler visualAssembler;

    [Header("Base")]
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

    public ScriptableObject GetEquippedPart(MechaPartType partType)
    {
        switch (partType)
        {
            case MechaPartType.Torse: return baseData;
            case MechaPartType.Tete: return headModule;
            case MechaPartType.BrasGauche: return leftArmModule;
            case MechaPartType.BrasDroit: return rightArmModule;
            case MechaPartType.JambeGauche: return leftLegModule;
            case MechaPartType.JambeDroite: return rightLegModule;
            default: return null;
        }
    }

    public void EquipPart(MechaPartType partType, ScriptableObject data)
    {
        GameObject prefabToEquip = null;

        if (partType == MechaPartType.Torse && data is MechaBaseData bd) 
        {
            baseData = bd;
            if (bd.Set != null)
            {
                prefabToEquip = bd.Set.GetPrefab(partType);
            }
        }
        else if (data is ModuleData md)
        {
            switch (partType)
            {
                case MechaPartType.Tete: headModule = md; break;
                case MechaPartType.BrasGauche: leftArmModule = md; break;
                case MechaPartType.BrasDroit: rightArmModule = md; break;
                case MechaPartType.JambeGauche: leftLegModule = md; break;
                case MechaPartType.JambeDroite: rightLegModule = md; break;
            }
            if (md.Set != null)
            {
                prefabToEquip = md.Set.GetPrefab(partType);
            }
        }

        if (visualAssembler != null && prefabToEquip != null)
        {
            visualAssembler.UpdatePartVisual(partType, prefabToEquip);
        }
    }

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