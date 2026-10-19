using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightEntity
{
    public event Action<FightEntity> OnDeath;
    public bool IsDead => CurrentHp <= 0;
    public EntityRuntimeData BaseStats { get; private set; }
    
    public int CurrentHp { get; private set; }
    public int Attack => GetEffectiveStat(BaseStats.Attack, Stat.Attack);
    public int Def => GetEffectiveStat(BaseStats.Def, Stat.Def);
    public int Speed => GetEffectiveStat(BaseStats.Speed, Stat.Speed);
    public int CritRate => GetEffectiveStat(BaseStats.CritRate, Stat.CritRate);
    public int CritDamage => GetEffectiveStat(BaseStats.CritDamage, Stat.CritDamage);
    
    private readonly List<StatModifier> activeModifiers = new();

    public FightEntity(EntityRuntimeData baseStats)
    {
        BaseStats = baseStats;
        CurrentHp = BaseStats.Hp;
    }
    
    private int GetEffectiveStat(int baseStatValue, Stat statType)
    {
        float flatSum = 0f;
        float percentSum = 0f;

        foreach (StatModifier modifier in activeModifiers)
        {
            if (modifier.Stat != statType) continue;

            if (modifier.Type == SubStatType.Flat)
                flatSum += modifier.Value;
            else
                percentSum += modifier.Value;
        }

        float result = (baseStatValue * (1f + percentSum / 100f)) + flatSum;
        return Mathf.Max(0, Mathf.RoundToInt(result));
    }
    
    public void AttackTarget(FightEntity target)
    {
        if (IsDead || target == null || target.IsDead) return;

        bool isCrit = Random.Range(0f, 100f) < CritRate;

        float rawDamage = Attack;
        if (isCrit)
        {
            rawDamage *= (CritDamage / 100f);
        }

        int finalDamage = Mathf.RoundToInt(rawDamage);

        target.TakeDamage(finalDamage);
    }

    #region PV
    
    public void TakeDamage(int amount)
    {
        int actualDamage = Mathf.Max(0, amount - Def);
    
        CurrentHp = Mathf.Max(0, CurrentHp - actualDamage);
        
        if (IsDead)
            Die();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHp = Mathf.Min(BaseStats.Hp, CurrentHp + amount);
    }

    private void Die() => OnDeath?.Invoke(this);
    
    #endregion

    #region Buff/Debuff
    
    public void AddModifier(StatModifier modifier)
    {
        activeModifiers.Add(modifier);
    }

    public void TickTurnEnd()
    {
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            activeModifiers[i].DurationTurns--;
            if (activeModifiers[i].DurationTurns <= 0)
            {
                activeModifiers.RemoveAt(i);
            }
        }
    }

    #endregion
}