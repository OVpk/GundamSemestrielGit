Shader "Custom/ColorPaletteUV"
{
    Properties
    {
        [Header(BaseColor)]
        _BaseGrayscale ("Grayscale Texture (UV0)", 2D) = "white" {}
        _ColorA ("Color A", Color) = (1,0,0,1)
        _ColorB ("Color B", Color) = (0,1,0,1)
        _ColorC ("Color C", Color) = (0,0,1,1)
        _ColorD ("Color D", Color) = (1,1,0,1)
        _ColorE ("Color E", Color) = (1,0,1,1)
        _ColorF ("Color F", Color) = (0,1,1,1)
        _ColorG ("Color G", Color) = (0,0,0,1)
        _ColorH ("Color H", Color) = (1,1,1,1)
        
        [Header(Shading)]
        _ShadowTint ("Shadow Tint", Color) = (0.4, 0.45, 0.6, 1.0)
        _ShadowThreshold ("Shadow Threshold", Range(-1.0, 1.0)) = 0.0
        _ShadowSmoothness ("Shadow Smoothness", Range(0.001, 0.2)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "CelShading.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                
                float3 normalWS : TEXCOORD3;
            };

            sampler2D _BaseGrayscale;
            
            CBUFFER_START(UnityPerMaterial)
            float4 _ColorA, _ColorB, _ColorC, _ColorD;
            float4 _ColorE, _ColorF, _ColorG, _ColorH;
            
            float4 _ShadowTint;
            float _ShadowThreshold;
            float _ShadowSmoothness;
            CBUFFER_END
            
            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv0 = input.uv0;
                output.uv1 = input.uv1;
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float grayscale = tex2D(_BaseGrayscale, input.uv0).r;

                int index = (int)floor(input.uv1.x * 8.0);
                index = clamp(index, 0, 7);
                
                float4 palette[8] = { _ColorA, _ColorB, _ColorC, _ColorD, _ColorE, _ColorF, _ColorG, _ColorH };
                float4 paletteColor = palette[index];
                
                float3 baseColor = (paletteColor * grayscale).rgb;
                
                float3 finalColor = ApplyCelShading(
                    baseColor,
                    input.normalWS,
                    _ShadowTint,
                    _ShadowThreshold,
                    _ShadowSmoothness);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}