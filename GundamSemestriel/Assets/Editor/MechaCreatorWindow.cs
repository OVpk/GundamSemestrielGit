using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MechaCreatorWindow : EditorWindow
{
    List<MechaBaseData> _mechasBase = new List<MechaBaseData>();
    List<ModuleData> _moduleDatas = new List<ModuleData>();
    
    MechaBaseData _selectedMechaBase;
    List<ModuleData> _selectedModules = new List<ModuleData>(5);
    
    Vector2 scrollPos =  Vector2.zero;
    
    [MenuItem("Tools/Mecha Creator")]
    static void Open()
    {
        MechaCreatorWindow customWindow = GetWindow<MechaCreatorWindow>("MechaCreator");
    }
    
    private void OnEnable()
    {
        Refresh();
    }
    
    private void OnProjectChange()
    {
        Refresh();
        Repaint();
    }

    private void Refresh()
    {
        _mechasBase.Clear();
        foreach (string GUID in AssetDatabase.FindAssets("t:MechaBaseData"))
        {
            string Path = AssetDatabase.GUIDToAssetPath(GUID);
            MechaBaseData mechaBase = AssetDatabase.LoadAssetAtPath<MechaBaseData>(Path);

            Debug.Log(mechaBase.name);

            _mechasBase.Add(mechaBase);
        }
        
        _moduleDatas.Clear();
        foreach (string GUID in AssetDatabase.FindAssets("t:ModuleData"))
        {
            string Path = AssetDatabase.GUIDToAssetPath(GUID);
            ModuleData module = AssetDatabase.LoadAssetAtPath<ModuleData>(Path);

            Debug.Log(module.name);

            _moduleDatas.Add(module);
        }
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.BeginVertical();

        if (_mechasBase.Count == 0)
        {
            EditorGUILayout.HelpBox("No mechaBase was found in the project", MessageType.Info);
        }
        else
        {
            foreach (MechaBaseData mechaBase in _mechasBase)
            {
                EditorGUILayout.LabelField(mechaBase.name);
            }
        }
        
        EditorGUILayout.Space();
        
        if (_moduleDatas.Count == 0)
        {
            EditorGUILayout.HelpBox("No mecha's module was found in the project", MessageType.Info);
        }
        else
        {
            foreach (ModuleData module in _moduleDatas)
            {
                EditorGUILayout.LabelField(module.name);
            }
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
}
