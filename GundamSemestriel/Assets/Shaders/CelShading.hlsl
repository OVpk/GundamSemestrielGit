#ifndef CEL_SHADING_INCLUDED
#define CEL_SHADING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float3 ApplyCelShading(float3 baseColor, float3 normalWS, float4 shadowTint, float shadowThreshold, float shadowSmoothness)
{
    Light mainLight = GetMainLight();
    float NdotL = dot(normalize(normalWS), mainLight.direction);
    float halfSmoothness = shadowSmoothness * 0.5;
    float toonLight = smoothstep(shadowThreshold - halfSmoothness, shadowThreshold + halfSmoothness, NdotL);
    
    float3 shadowColor = baseColor * shadowTint.rgb;
    
    return lerp(shadowColor, baseColor * mainLight.color, toonLight);
}

#endif