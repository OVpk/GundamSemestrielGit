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

    public List<GameObject> GetPrefabsByPart(MechaPartType partType)
    {
        List<GameObject> prefabs = new List<GameObject>();

        switch (partType)
        {
            case MechaPartType.Torse:
                foreach (var data in torses) if (data != null && data.prefab != null) prefabs.Add(data.prefab);
                break;
            case MechaPartType.Tete:
                foreach (var data in tetes) if (data != null && data.prefab != null) prefabs.Add(data.prefab);
                break;
            case MechaPartType.BrasGauche:
                foreach (var data in brasGauches) if (data != null && data.prefab != null) prefabs.Add(data.prefab);
                break;
            case MechaPartType.BrasDroit:
                foreach (var data in brasDroits) if (data != null && data.prefab != null) prefabs.Add(data.prefab);
                break;
            case MechaPartType.JambeGauche:
                foreach (var data in jambesGauches) if (data != null && data.prefab != null) prefabs.Add(data.prefab);
                break;
            case MechaPartType.JambeDroite:
                foreach (var data in jambesDroites) if (data != null && data.prefab != null) prefabs.Add(data.prefab);
                break;
        }

        return prefabs;
    }
}