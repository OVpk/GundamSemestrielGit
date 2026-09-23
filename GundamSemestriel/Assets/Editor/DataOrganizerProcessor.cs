using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DataOrganizerProcessor : AssetPostprocessor
{
    private class SelfFolderRule
    {
        public Type TargetType;
        public string BaseFolderPath;
    }

    private class FollowReferenceRule
    {
        public Type TargetType;
        public string FieldName;
    }

    private static readonly SelfFolderRule[] SelfFolderRules = new SelfFolderRule[]
    {
        new SelfFolderRule { TargetType = typeof(SetData), BaseFolderPath = "Assets/Data/DataBase/Sets" },
        
    };

    private static readonly FollowReferenceRule[] FollowReferenceRules = new FollowReferenceRule[]
    {
        new FollowReferenceRule { TargetType = typeof(ModuleData), FieldName = "set" }
    };

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            if (!path.EndsWith(".asset")) continue;

            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null) continue;

            Type assetType = asset.GetType();

            foreach (var rule in SelfFolderRules)
            {
                if (rule.TargetType.IsAssignableFrom(assetType))
                {
                    ProcessSelfFolder(asset, path, rule);
                    break;
                }
            }

            foreach (var rule in FollowReferenceRules)
            {
                if (rule.TargetType.IsAssignableFrom(assetType))
                {
                    ProcessFollowReference(asset, path, rule);
                    break;
                }
            }
        }
    }

    private static void ProcessSelfFolder(UnityEngine.Object asset, string currentPath, SelfFolderRule rule)
    {
        string assetName = Path.GetFileNameWithoutExtension(currentPath);
        string targetFolder = $"{rule.BaseFolderPath}/{assetName}";
        string targetPath = $"{targetFolder}/{assetName}.asset";

        if (currentPath == targetPath) return;

        EnsureFolderExists(targetFolder);

        EditorApplication.delayCall += () =>
        {
            if (asset != null && AssetDatabase.GetAssetPath(asset) != targetPath)
            {
                AssetDatabase.MoveAsset(currentPath, targetPath);
                Debug.Log($"[DataOrganizer] Dossier créé et '{assetName}' déplacé dans : {targetFolder}");
            }
        };
    }

    private static void ProcessFollowReference(UnityEngine.Object asset, string currentPath, FollowReferenceRule rule)
    {
        SerializedObject serializedAsset = new SerializedObject(asset);
        SerializedProperty prop = serializedAsset.FindProperty(rule.FieldName);

        if (prop == null || prop.propertyType != SerializedPropertyType.ObjectReference) return;
        if (prop.objectReferenceValue == null) return;

        string referencedAssetPath = AssetDatabase.GetAssetPath(prop.objectReferenceValue);
        if (string.IsNullOrEmpty(referencedAssetPath)) return;

        string targetFolder = Path.GetDirectoryName(referencedAssetPath)?.Replace("\\", "/");
        string fileName = Path.GetFileName(currentPath);
        string targetPath = $"{targetFolder}/{fileName}";

        if (currentPath == targetPath) return;

        EditorApplication.delayCall += () =>
        {
            if (asset != null && AssetDatabase.GetAssetPath(asset) != targetPath)
            {
                AssetDatabase.MoveAsset(currentPath, targetPath);
                Debug.Log($"[DataOrganizer] Asset '{asset.name}' déplacé vers : {targetFolder}");
            }
        };
    }

    private static void EnsureFolderExists(string targetPath)
    {
        if (AssetDatabase.IsValidFolder(targetPath)) return;

        string[] folders = targetPath.Split('/');
        string currentPath = folders[0];

        for (int i = 1; i < folders.Length; i++)
        {
            string nextFolder = $"{currentPath}/{folders[i]}";
            if (!AssetDatabase.IsValidFolder(nextFolder))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }
            currentPath = nextFolder;
        }
    }
}