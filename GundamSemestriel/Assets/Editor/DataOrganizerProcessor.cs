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
        public string[] SubfoldersToCreate;
    }

    private class FollowReferenceRule
    {
        public Type TargetType;
        public string FieldName;
        public string TargetSubfolder;
        public string FallbackFolderPath;
    }

    private static readonly SelfFolderRule[] SelfFolderRules = new SelfFolderRule[]
    {
        new SelfFolderRule 
        { 
            TargetType = typeof(SetData), 
            BaseFolderPath = "Assets/Data/DataBase/Sets",
            SubfoldersToCreate = new string[] { "Modules" }
        },
    };

    private static readonly FollowReferenceRule[] FollowReferenceRules = new FollowReferenceRule[]
    {
        new FollowReferenceRule 
        { 
            TargetType = typeof(ModuleData), 
            FieldName = "set",
            TargetSubfolder = "Modules",
            FallbackFolderPath = "Assets/Data/DataBase/Sets/Default" 
        }
    };

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string deletedPath in deletedAssets)
        {
            if (!deletedPath.EndsWith(".asset")) continue;

            string assetName = Path.GetFileNameWithoutExtension(deletedPath);
            string normalizedDeletedPath = deletedPath.Replace("\\", "/");

            foreach (var rule in SelfFolderRules)
            {
                string expectedFolder = $"{rule.BaseFolderPath}/{assetName}";
                string expectedPath = $"{expectedFolder}/{assetName}.asset";

                if (normalizedDeletedPath == expectedPath)
                {
                    HandleDeletion(expectedFolder, assetName);
                }
            }
        }

        for (int i = 0; i < movedAssets.Length; i++)
        {
            string newPath = movedAssets[i].Replace("\\", "/");
            string oldPath = movedFromAssetPaths[i].Replace("\\", "/");

            if (!newPath.EndsWith(".asset")) continue;

            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(newPath);
            if (asset == null) continue;

            Type assetType = asset.GetType();

            foreach (var rule in SelfFolderRules)
            {
                if (rule.TargetType.IsAssignableFrom(assetType))
                {
                    HandleRename(oldPath, newPath, rule);
                    break;
                }
            }
        }

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

    private static void HandleDeletion(string folderPath, string deletedAssetName)
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { folderPath });
                
                foreach (string guid in guids)
                {
                    string objPath = AssetDatabase.GUIDToAssetPath(guid);
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(objPath);
                    
                    if (obj != null)
                    {
                        foreach (var fRule in FollowReferenceRules)
                        {
                            if (fRule.TargetType.IsAssignableFrom(obj.GetType()) && !string.IsNullOrEmpty(fRule.FallbackFolderPath))
                            {
                                string fileName = Path.GetFileName(objPath);
                                EnsureFolderExists(fRule.FallbackFolderPath);
                                
                                AssetDatabase.MoveAsset(objPath, $"{fRule.FallbackFolderPath}/{fileName}");
                                Debug.Log($"[DataOrganizer] '{fileName}' transféré vers '{fRule.FallbackFolderPath}' suite à la suppression de '{deletedAssetName}'.");
                            }
                        }
                    }
                }

                AssetDatabase.DeleteAsset(folderPath);
                Debug.Log($"[DataOrganizer] Dossier de Set '{folderPath}' supprimé proprement.");
            }
        };
    }

    private static void HandleRename(string oldPath, string newPath, SelfFolderRule rule)
    {
        string oldName = Path.GetFileNameWithoutExtension(oldPath);
        string newName = Path.GetFileNameWithoutExtension(newPath);

        if (oldName != newName)
        {
            string oldFolder = $"{rule.BaseFolderPath}/{oldName}";
            
            if (AssetDatabase.IsValidFolder(oldFolder))
            {
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.RenameAsset(oldFolder, newName);
                    Debug.Log($"[DataOrganizer] SetData renommé. Dossier '{oldName}' renommé en '{newName}'.");
                };
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

        if (rule.SubfoldersToCreate != null)
        {
            foreach (string subfolder in rule.SubfoldersToCreate)
            {
                EnsureFolderExists($"{targetFolder}/{subfolder}");
            }
        }

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

        string targetFolder;

        if (prop.objectReferenceValue != null)
        {
            string referencedAssetPath = AssetDatabase.GetAssetPath(prop.objectReferenceValue);
            if (string.IsNullOrEmpty(referencedAssetPath)) return;

            targetFolder = Path.GetDirectoryName(referencedAssetPath)?.Replace("\\", "/");

            if (!string.IsNullOrEmpty(rule.TargetSubfolder))
            {
                targetFolder = $"{targetFolder}/{rule.TargetSubfolder}";
            }
        }
        else
        {
            if (string.IsNullOrEmpty(rule.FallbackFolderPath)) return;
            targetFolder = rule.FallbackFolderPath;
        }

        EnsureFolderExists(targetFolder);

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