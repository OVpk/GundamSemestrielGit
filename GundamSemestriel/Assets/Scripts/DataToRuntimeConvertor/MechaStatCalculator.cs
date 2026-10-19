using System;
using System.Collections.Generic;
using UnityEngine;

public static class MechaStatCalculator
{
    private class StatAccumulator
    {
        public float FlatSum;
        public float PercentSum;
    }

    public static EntityRuntimeData CalculateRuntimeData(MechaBaseData baseData, IEnumerable<ModuleData> modules)
    {
        if (baseData == null)
        {
            Debug.LogError("MechaBaseData == null");
            return default;
        }

        var accumulators = new Dictionary<Stat, StatAccumulator>();
        foreach (Stat stat in Enum.GetValues(typeof(Stat)))
        {
            accumulators[stat] = new StatAccumulator();
        }

        var setCounts = new Dictionary<SetData, int>();

        foreach (var module in modules)
        {
            if (module == null) continue;
            
            if (module.Set != null)
            {
                setCounts.TryAdd(module.Set, 0);
                setCounts[module.Set]++;
            }

            if (module.SubStats != null)
            {
                foreach (SubStat subStat in module.SubStats)
                {
                    AddSubStatToAccumulator(accumulators, subStat);
                }
            }
        }

        foreach (var setAndCount in setCounts)
        {
            SetData setData = setAndCount.Key;
            int equippedCount = setAndCount.Value;

            if (setData.SetBonus == null) continue;

            foreach (ConditionnalSubstat conditionalBonus in setData.SetBonus)
            {
                if (equippedCount >= conditionalBonus.RequiredSetNb)
                {
                    AddSubStatToAccumulator(accumulators, conditionalBonus.SubStat);
                }
            }
        }

        int finalHp = CalculateFinalValue(baseData.Hp, accumulators[Stat.Hp]);
        int finalAttack = CalculateFinalValue(baseData.Attack, accumulators[Stat.Attack]);
        int finalDef = CalculateFinalValue(baseData.Def, accumulators[Stat.Def]);
        int finalSpeed = CalculateFinalValue(baseData.Speed, accumulators[Stat.Speed]);
        int finalCritRate = CalculateFinalValue(baseData.CritRate, accumulators[Stat.CritRate]);
        int finalCritDamage = CalculateFinalValue(baseData.CritDamage, accumulators[Stat.CritDamage]);

        return new EntityRuntimeData(finalHp, finalAttack, finalDef, finalSpeed, finalCritRate, finalCritDamage);
    }

    private static void AddSubStatToAccumulator(Dictionary<Stat, StatAccumulator> accumulators, SubStat subStat)
    {
        if (!accumulators.TryGetValue(subStat.Stat, out var acc)) return;

        if (subStat.AdditiveType == SubStatType.Flat)
        {
            acc.FlatSum += subStat.StatValue;
        }
        else if (subStat.AdditiveType == SubStatType.Percent)
        {
            acc.PercentSum += subStat.StatValue;
        }
    }

    private static int CalculateFinalValue(int baseValue, StatAccumulator acc)
    {
        float percentMultiplier = 1f + (acc.PercentSum / 100f);
        float result = (baseValue * percentMultiplier) + acc.FlatSum;
        return Mathf.RoundToInt(result);
    }
}