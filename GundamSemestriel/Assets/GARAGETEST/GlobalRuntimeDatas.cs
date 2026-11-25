using System;
using UnityEngine;

public class GlobalRuntimeDatas : MonoBehaviour
{
    public static GlobalRuntimeDatas Instance;

    public MechaTestProto[] mechas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    
}
