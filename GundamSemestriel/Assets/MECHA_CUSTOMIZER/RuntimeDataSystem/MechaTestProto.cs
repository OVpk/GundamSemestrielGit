using System.Collections.Generic;
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
    
    public int GetEquippedSetCount(SetData targetSet)
    {
        if (targetSet == null) return 0;

        int count = 0;
        if (baseData != null && baseData.Set == targetSet) count++;

        foreach (ModuleData module in Modules)
        {
            if (module != null && module.Set == targetSet) count++;
        }

        return count;
    }
    
    public EntityRuntimeData GetCalculatedStats()
    {
        return new EntityRuntimeData(BaseData, Modules);
    }

    public Dictionary<SetData, int> GetEquippedSetsCounts()
    {
        var counts = new Dictionary<SetData, int>();

        if (baseData != null && baseData.Set != null)
        {
            counts[baseData.Set] = 1;
        }

        foreach (ModuleData module in Modules)
        {
            if (module == null || module.Set == null) continue;

            if (counts.ContainsKey(module.Set))
                counts[module.Set]++;
            else
                counts[module.Set] = 1;
        }

        return counts;
    }
    
    public EntityRuntimeData GetSimulatedStats(MechaPartType partType, ScriptableObject candidatePart)
    {
        MechaBaseData simBase = baseData;
        ModuleData simHead = headModule;
        ModuleData simLeftArm = leftArmModule;
        ModuleData simRightArm = rightArmModule;
        ModuleData simLeftLeg = leftLegModule;
        ModuleData simRightLeg = rightLegModule;

        if (partType == MechaPartType.Torse && candidatePart is MechaBaseData bd)
        {
            simBase = bd;
        }
        else if (candidatePart is ModuleData md)
        {
            switch (partType)
            {
                case MechaPartType.Tete: simHead = md; break;
                case MechaPartType.BrasGauche: simLeftArm = md; break;
                case MechaPartType.BrasDroit: simRightArm = md; break;
                case MechaPartType.JambeGauche: simLeftLeg = md; break;
                case MechaPartType.JambeDroite: simRightLeg = md; break;
            }
        }

        ModuleData[] simModules = new ModuleData[]
        {
            simHead,
            simLeftArm,
            simRightArm,
            simLeftLeg,
            simRightLeg
        };

        return new EntityRuntimeData(simBase, simModules);
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