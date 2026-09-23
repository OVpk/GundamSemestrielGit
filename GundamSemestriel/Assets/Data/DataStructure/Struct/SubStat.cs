using System;
using UnityEngine;

[Serializable]
public struct SubStat
{
    private enum SubStatType 
    {
        Flat,
        Percent
    }
    
    [SerializeField] private SubStatType additiveType;
    [SerializeField] private int statValue;
    [SerializeField] private Stat stat;
}
