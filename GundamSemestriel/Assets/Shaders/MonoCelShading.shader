Shader "Custom/MonoCelShading"
{
    Properties
    {
        _MainTex ("Main Texture (RGB)", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Header(Cel Shading Settings)]
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
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _BaseColor;

            float4 _ShadowTint;
            float _ShadowThreshold;
            float _ShadowSmoothness;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float4 texColor = tex2D(_MainTex, input.uv) * _BaseColor;
                float3 baseColor = texColor.rgb;
                float3 finalColor = ApplyCelShading(
                    baseColor,
                    input.normalWS,
                    _ShadowTint,
                    _ShadowThreshold,
                    _ShadowSmoothness
                );

                return float4(finalColor, texColor.a);
            }
            ENDHLSL
        }
    }
}