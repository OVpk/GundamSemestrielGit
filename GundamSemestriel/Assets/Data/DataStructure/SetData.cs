using UnityEngine;

[CreateAssetMenu(menuName = "Data/Mecha/Set")]
public class SetData : ScriptableObject
{
    [field: SerializeField] public ModuleSet Set { get; private set; }
    [field: SerializeField] public ConditionnalSubstat[] SetBonus { get; private set; }

    [Header("Prefabs des parties du Set")]
    [SerializeField] private GameObject headPrefab;
    [SerializeField] private GameObject torsoPrefab;
    [SerializeField] private GameObject leftArmPrefab;
    [SerializeField] private GameObject rightArmPrefab;
    [SerializeField] private GameObject leftLegPrefab;
    [SerializeField] private GameObject rightLegPrefab;

    public GameObject GetPrefab(MechaPartType partType)
    {
        return partType switch
        {
            MechaPartType.Tete => headPrefab,
            MechaPartType.Torse => torsoPrefab,
            MechaPartType.BrasGauche => leftArmPrefab,
            MechaPartType.BrasDroit => rightArmPrefab,
            MechaPartType.JambeGauche => leftLegPrefab,
            MechaPartType.JambeDroite => rightLegPrefab,
            _ => null
        };
    }
}