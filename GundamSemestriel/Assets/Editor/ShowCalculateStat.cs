using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MechaTestProto))]
public class ShowCalculateStat : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MechaTestProto script = (MechaTestProto)target;

        if (GUILayout.Button("Calculer les stats finales"))
        {
            script.DisplayCalculatedStats();
        }
    }
}
