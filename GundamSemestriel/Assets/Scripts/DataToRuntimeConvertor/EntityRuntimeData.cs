public readonly struct EntityRuntimeData
{
    public int Hp { get; }
    public int Attack { get; }
    public int Def { get; }
    public int Speed { get; }
    public int CritRate { get; }
    public int CritDamage { get; }

    //TO DO LE METTRE EN PRIVATE ET GERER LES ASSEMBLY DEF
    public EntityRuntimeData(int hp, int attack, int def, int speed, int critRate, int critDamage)
    {
        Hp = hp;
        Attack = attack;
        Def = def;
        Speed = speed;
        CritRate = critRate;
        CritDamage = critDamage;
    }

    public EntityRuntimeData(EnemyData enemy)
        : this(enemy.Hp, enemy.Attack, enemy.Def, enemy.Speed, enemy.CritRate, enemy.CritDamage)
    {
    }
    
    public EntityRuntimeData(MechaBaseData mechaBase, ModuleData[] mechaModules)
    {
        this = MechaStatCalculator.CalculateRuntimeData(mechaBase, mechaModules);
    }
}