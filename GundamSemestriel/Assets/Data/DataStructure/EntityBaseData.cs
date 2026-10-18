using UnityEngine;

public abstract class EntityBaseData : ScriptableObject
{
    [Header("Global Stats")]
    [field: SerializeField] public int Hp { get; private set; }
    [field: SerializeField] public int Attack { get; private set; }
    [field: SerializeField] public int Def { get; private set; }
    [field: SerializeField] public int Speed { get; private set; }
    [field: SerializeField] public int CritRate { get; private set; }
    [field: SerializeField] public int CritDamage { get; private set; }
}