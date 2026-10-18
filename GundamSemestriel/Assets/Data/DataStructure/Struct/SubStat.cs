using System;
using UnityEngine;

public enum SubStatType { Flat, Percent }

[Serializable]
public struct SubStat
{
    [field: SerializeField] public SubStatType AdditiveType { get; private set; }
    [field: SerializeField] public int StatValue { get; private set; }
    [field: SerializeField] public Stat Stat { get; private set; }
}

[Serializable]
public struct ConditionnalSubstat
{
    [field: SerializeField] public int RequiredSetNb { get; private set; }
    [field: SerializeField] public SubStat SubStat { get; private set; }
}
