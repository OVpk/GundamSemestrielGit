using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MechaCutterWindow : EditorWindow
{
    private GameObject sourcePrefab;
    private GameObject currentPrefab;
    
    // Chemin de sauvegarde par défaut
    private string outputFolderPath = "Assets"; 

    private Dictionary<string, bool> partsSelection = new Dictionary<string, bool>();
    private const string RigName = "Mecha_Rig";

    [MenuItem("Tools/Mecha Cutter")]
    public static void ShowWindow()
    {
        GetWindow<MechaCutterWindow>("Mecha Cutter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Paramètres de découpe", EditorStyles.boldLabel);

        sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Mecha Prefab", sourcePrefab, typeof(GameObject), false);

        if (sourcePrefab != currentPrefab)
        {
            currentPrefab = sourcePrefab;
            RefreshPartsList();
        }

        if (sourcePrefab == null)
        {
            EditorGUILayout.HelpBox("Veuillez assigner un Prefab depuis vos assets.", MessageType.Info);
            return;
        }

        // --- NOUVELLE SECTION : Sélection du dossier ---
        GUILayout.Space(10);
        GUILayout.Label("Dossier de destination :", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        // Affiche le chemin actuel
        EditorGUILayout.TextField(outputFolderPath);
        
        // Bouton pour ouvrir la fenêtre de sélection de dossier
        if (GUILayout.Button("Parcourir...", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Sélectionner le dossier de destination", outputFolderPath, "");
            
            if (!string.IsNullOrEmpty(path))
            {
                // Unity a besoin d'un chemin relatif (commençant par "Assets/") pour sauvegarder un Prefab
                if (path.StartsWith(Application.dataPath))
                {
                    outputFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogWarning("Veuillez sélectionner un dossier situé à l'intérieur du dossier Assets de ce projet.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        // ------------------------------------------------

        GUILayout.Space(10);
        GUILayout.Label("Parties à CONSERVER (Mecha_Rig est toujours conservé) :", EditorStyles.boldLabel);

        List<string> partNames = new List<string>(partsSelection.Keys);
        foreach (string partName in partNames)
        {
            partsSelection[partName] = EditorGUILayout.Toggle(partName, partsSelection[partName]);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Cut", GUILayout.Height(40)))
        {
            GenerateCutPrefab();
        }
    }

    private void RefreshPartsList()
    {
        partsSelection.Clear();
        if (sourcePrefab == null) return;

        foreach (Transform child in sourcePrefab.transform)
        {
            if (child.name != RigName)
            {
                partsSelection.Add(child.name, true);
            }
        }
    }

    private void GenerateCutPrefab()
    {
        string originalPath = AssetDatabase.GetAssetPath(sourcePrefab);
        if (string.IsNullOrEmpty(originalPath))
        {
            Debug.LogError("Le GameObject assigné n'est pas un Prefab valide de vos assets.");
            return;
        }

        // Construction du nouveau chemin en utilisant le dossier sélectionné
        string prefabName = sourcePrefab.name + "_Cut.prefab";
        string newPath = outputFolderPath + "/" + prefabName;
        
        // S'assure de ne pas écraser un fichier existant du même nom
        newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        List<GameObject> toDestroy = new List<GameObject>();

        foreach (Transform child in instance.transform)
        {
            if (child.name == RigName)
                continue;

            if (partsSelection.TryGetValue(child.name, out bool keepPart))
            {
                if (!keepPart)
                {
                    toDestroy.Add(child.gameObject);
                }
            }
        }

        foreach (GameObject obj in toDestroy)
        {
            DestroyImmediate(obj);
        }

        // Sauvegarde dans le dossier choisi
        PrefabUtility.SaveAsPrefabAsset(instance, newPath);
        DestroyImmediate(instance);

        Debug.Log("Nouveau Prefab généré avec succès dans : " + newPath);
        
        Object newAsset = AssetDatabase.LoadAssetAtPath<Object>(newPath);
        Selection.activeObject = newAsset;
        EditorGUIUtility.PingObject(newAsset);
    }
}