public class StatModifier
{
    public Stat Stat { get; }
    public SubStatType Type { get; }
    public float Value { get; }
    public int DurationTurns { get; set; }

    public StatModifier(Stat stat, SubStatType type, float value, int durationTurns)
    {
        Stat = stat;
        Type = type;
        Value = value;
        DurationTurns = durationTurns;
    }
}