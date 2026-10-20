using UnityEngine;

[CreateAssetMenu(menuName = "Data/Palette")]
public class ColorPaletteData : ScriptableObject
{
    [field: SerializeField] public Color colorA { get; private set; }
    [field: SerializeField] public Color colorB { get; private set; }
    [field: SerializeField] public Color colorC { get; private set; }
    [field: SerializeField] public Color colorD { get; private set; }
    [field: SerializeField] public Color colorE { get; private set; }
    [field: SerializeField] public Color colorF { get; private set; }
    [field: SerializeField] public Color colorG { get; private set; }
    [field: SerializeField] public Color colorH { get; private set; }
    
#if UNITY_EDITOR
    public void SaveColorsForEditor(Color a, Color b, Color c, Color d, Color e, Color f, Color g, Color h)
    {
        colorA = a; colorB = b; colorC = c; colorD = d; colorE = e; colorF = f; colorG = g; colorH = h;
    }
#endif
}