using UnityEngine;

[RequireComponent(typeof(Renderer))] 
public class PaletteApplierTest : MonoBehaviour
{
    private static readonly int ColorA_ID = Shader.PropertyToID("_ColorA");
    private static readonly int ColorB_ID = Shader.PropertyToID("_ColorB");
    private static readonly int ColorC_ID = Shader.PropertyToID("_ColorC");
    
    [SerializeField] private ColorPaletteData testPalette;
    private Renderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    private void Init()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<Renderer>();

        propertyBlock ??= new MaterialPropertyBlock();
    }

    public void ApplyPalette(ColorPaletteData palette)
    {
        Init();

        if (palette == null) 
        {
            meshRenderer.SetPropertyBlock(null);
            return;
        }

        meshRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor(ColorA_ID, palette.colorA);
        propertyBlock.SetColor(ColorB_ID, palette.colorB);
        propertyBlock.SetColor(ColorC_ID, palette.colorC);

        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif
        
        ApplyPalette(testPalette);
    }
}