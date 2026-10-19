Shader "Custom/Outline"
{
    Properties
    {
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.005
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color    : COLOR; 
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            float _OutlineWidth;
            float4 _OutlineColor;
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 decodedNormal = input.color.rgb * 2.0 - 1.0;
                
                float3 smoothNormalOS = float3(-decodedNormal.x, decodedNormal.y, decodedNormal.z);
                
                float thickness = input.color.a * _OutlineWidth;
                float3 inflatedPositionOS = input.positionOS.xyz + (smoothNormalOS * thickness);
                
                output.positionCS = TransformObjectToHClip(inflatedPositionOS);
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}