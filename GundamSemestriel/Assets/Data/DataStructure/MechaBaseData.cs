using UnityEngine;

public class MechaBaseData : MonoBehaviour
{
    [field: SerializeField] public SetData Set { get; private set; }
    
    [field: SerializeField] public int Hp { get; private set; }
    [field: SerializeField] public int Attack { get; private set; }
    [field: SerializeField] public int Def { get; private set; }
    [field: SerializeField] public int Speed { get; private set; }
    [field: SerializeField] public int Crit { get; private set; }

}
