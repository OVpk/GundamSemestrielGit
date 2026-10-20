using UnityEngine;
using UnityEditor;
using System.IO;

public class PaletteMaterialEditor : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);

        Material targetMat = materialEditor.target as Material;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate a color palette", GUILayout.Height(30)))
        {
            EditorApplication.delayCall += () => GeneratePaletteAsset(targetMat);
        }
    }

    private void GeneratePaletteAsset(Material mat)
    {
        string folderPath = "Assets/Data/DataBase/ColorPalettes";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        string chosenPath = EditorUtility.SaveFilePanelInProject(
            "Save PaletteData file",
            mat.name + "_Palette",
            "asset",
            "Name file :",
            folderPath 
        );

        if (string.IsNullOrEmpty(chosenPath)) return;

        string fileName = Path.GetFileName(chosenPath);
        string finalPath = folderPath + "/" + fileName;

        ColorPaletteData newPalette = ScriptableObject.CreateInstance<ColorPaletteData>();
        
        newPalette.SaveColorsForEditor(
            mat.GetColor("_ColorA"),
            mat.GetColor("_ColorB"),
            mat.GetColor("_ColorC"),
            mat.GetColor("_ColorD"),
            mat.GetColor("_ColorE"),
            mat.GetColor("_ColorF"),
            mat.GetColor("_ColorG"),
            mat.GetColor("_ColorH")
        );

        AssetDatabase.CreateAsset(newPalette, finalPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newPalette;
    }
}