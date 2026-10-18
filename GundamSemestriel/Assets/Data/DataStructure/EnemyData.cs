using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy/Enemy")]
public class EnemyData : EntityBaseData
{
    [Header("Enemy Reward (not implemented)")]
    [SerializeField] private int reward;

    public EntityRuntimeData ToRuntimeData()
    {
        return new EntityRuntimeData(Hp, Attack, Def, Speed, CritRate, CritDamage);
    }
}