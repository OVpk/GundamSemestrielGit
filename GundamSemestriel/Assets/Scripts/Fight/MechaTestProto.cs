using UnityEngine;

public class MechaTestProto : MonoBehaviour
{
    [SerializeField] private MechaBaseData baseData;
    [SerializeField] private ModuleData[] modules = new ModuleData[5];
    
    public MechaBaseData BaseData => baseData;
    public ModuleData[] Modules => modules;
}