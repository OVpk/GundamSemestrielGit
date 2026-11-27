using System.Collections.Generic;
using UnityEngine;

public enum MechaPartType
{
    Tete,
    Torse,
    BrasGauche,
    BrasDroit,
    JambeGauche,
    JambeDroite
}

[CreateAssetMenu(menuName = "Data/Mecha/Inventory")]
public class MechaInventory : ScriptableObject
{
    [Header("Torso (MechaBaseData)")]
    public List<MechaBaseData> torses = new List<MechaBaseData>();

    [Header("Modules (ModuleData)")]
    public List<ModuleData> tetes = new List<ModuleData>();
    public List<ModuleData> brasGauches = new List<ModuleData>();
    public List<ModuleData> brasDroits = new List<ModuleData>();
    public List<ModuleData> jambesGauches = new List<ModuleData>();
    public List<ModuleData> jambesDroites = new List<ModuleData>();

    public List<SetData> GetOwnedSetsForPart(MechaPartType partType)
    {
        List<SetData> ownedSets = new List<SetData>();

        if (partType == MechaPartType.Torse)
        {
            foreach (var data in torses)
            {
                if (data != null && data.Set != null && !ownedSets.Contains(data.Set))
                    ownedSets.Add(data.Set);
            }
        }
        else
        {
            List<ModuleData> modules = GetModuleList(partType);
            foreach (var data in modules)
            {
                if (data != null && data.Set != null && !ownedSets.Contains(data.Set))
                    ownedSets.Add(data.Set);
            }
        }

        return ownedSets;
    }

    public List<ScriptableObject> GetItemsByPartAndSet(MechaPartType partType, SetData targetSet)
    {
        List<ScriptableObject> results = new List<ScriptableObject>();
        if (targetSet == null) return results;

        if (partType == MechaPartType.Torse)
        {
            foreach (var data in torses)
            {
                if (data != null && data.Set == targetSet)
                    results.Add(data);
            }
        }
        else
        {
            List<ModuleData> modules = GetModuleList(partType);
            foreach (var data in modules)
            {
                if (data != null && data.Set == targetSet)
                    results.Add(data);
            }
        }

        return results;
    }

    private List<ModuleData> GetModuleList(MechaPartType partType)
    {
        return partType switch
        {
            MechaPartType.Tete => tetes,
            MechaPartType.BrasGauche => brasGauches,
            MechaPartType.BrasDroit => brasDroits,
            MechaPartType.JambeGauche => jambesGauches,
            MechaPartType.JambeDroite => jambesDroites,
            _ => new List<ModuleData>()
        };
    }
}