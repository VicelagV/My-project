Shader "AllAboard/Stylized Train"
{
    Properties
    {
        _BaseColor ("Paint", Color) = (1,1,1,1)
        _EmissionColor ("Glow", Color) = (0,0,0,1)
        _BaseMap ("Surface Grain", 2D) = "white" {}
        _GrainScale ("Grain Scale", Float) = 0.7
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // Use ordinary material draws: Unity 6000.6 SRP batching reuses paint
        // constants between these custom shader materials. Spatial world meshes
        // already batch geometry without changing renderer-wide project settings.
        float4 _BaseColor;
        float4 _EmissionColor;
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        float _GrainScale;
        ENDHLSL
        Pass
        {
            Name "TrainColour"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float4 shadowCoord:TEXCOORD2; half fog:TEXCOORD3; float3 grainPosition:TEXCOORD4; half3 grainNormal:TEXCOORD5; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                // Preserve physical grain scale on stretched primitives, while moving with the object.
                float3 axisScale=float3(length(UNITY_MATRIX_M._m00_m10_m20),length(UNITY_MATRIX_M._m01_m11_m21),length(UNITY_MATRIX_M._m02_m12_m22));
                output.grainPosition=input.positionOS.xyz*axisScale;
                output.grainNormal=input.normalOS;
                output.positionWS=TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS=TransformWorldToHClip(output.positionWS);
                output.normalWS=TransformObjectToWorldNormal(input.normalOS);
                output.shadowCoord=TransformWorldToShadowCoord(output.positionWS);
                output.fog=ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half3 normal=normalize(input.normalWS);
                Light sun=GetMainLight(input.shadowCoord);
                half ndl=saturate(dot(normal,sun.direction));
                half band=lerp(0.32h,1.0h,smoothstep(0.0h,1.0h,ndl));
                half3 lighting=SampleSH(normal)*1.1h+sun.color*band*lerp(0.45h,1.0h,sun.shadowAttenuation);
                #if defined(_ADDITIONAL_LIGHTS)
                uint count=GetAdditionalLightsCount();
                for(uint i=0;i<count;i++)
                {
                    Light lamp=GetAdditionalLight(i,input.positionWS);
                    lighting+=lamp.color*saturate(dot(normal,lamp.direction))*lamp.distanceAttenuation*lamp.shadowAttenuation;
                }
                #endif
                half3 weights=pow(abs(normalize(input.grainNormal)),4);
                weights/=max(dot(weights,half3(1,1,1)),0.001h);
                float3 p=input.grainPosition*_GrainScale;
                half3 grain=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.yz).rgb*weights.x
                    +SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xz).rgb*weights.y
                    +SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xy).rgb*weights.z;
                return half4(MixFog(_BaseColor.rgb*grain*lighting+_EmissionColor.rgb,input.fog),_BaseColor.a);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection;
            float3 _LightPosition;
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            float4 ShadowVert(Attributes input):SV_POSITION
            {
                float3 position=TransformObjectToWorld(input.positionOS.xyz);
                float3 normal=TransformObjectToWorldNormal(input.normalOS);
                float3 direction=_LightDirection;
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                direction=normalize(_LightPosition-position);
                #endif
                float4 clip=TransformWorldToHClip(ApplyShadowBias(position,normal,direction));
                #if UNITY_REVERSED_Z
                clip.z=min(clip.z,UNITY_NEAR_CLIP_VALUE*clip.w);
                #else
                clip.z=max(clip.z,UNITY_NEAR_CLIP_VALUE*clip.w);
                #endif
                return clip;
            }
            half4 ShadowFrag():SV_Target { return 0; }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 DepthVert(float4 position:POSITION):SV_POSITION { return TransformObjectToHClip(position.xyz); }
            half4 DepthFrag():SV_Target { return 0; }
            ENDHLSL
        }
    }
}
