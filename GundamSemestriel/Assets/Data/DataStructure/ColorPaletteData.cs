using UnityEngine;

[CreateAssetMenu(menuName = "Data/Palette")]
public class ColorPaletteData : ScriptableObject
{
    [field: SerializeField] public Color colorA { get; private set; }
    [field: SerializeField] public Color colorB { get; private set; }
    [field: SerializeField] public Color colorC { get; private set; }
}