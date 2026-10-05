Shader "TopGun/Alpine/SurfaceSnow"
{
    Properties
    {
        _BaseMap ("Leaf alpha", 2D) = "white" {}
        _SnowColor ("Snow color", Color) = (0.82, 0.89, 0.94, 1)
        _Cutoff ("Leaf cutoff", Range(0,1)) = 0.4
        _UpBias ("Branch snow", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest+10" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _SnowColor;
            half _Cutoff, _UpBias;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; half fog:TEXCOORD3; };
            Varyings Vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS;
                o.normalWS=TransformObjectToWorldNormal(input.normalOS);
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap); o.fog=ComputeFogFactor(p.positionCS.z);
                return o;
            }
            half4 Frag(Varyings input):SV_Target
            {
                clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv).a-_Cutoff);
                half3 n=normalize(input.normalWS);
                float3 p=input.positionWS;
                half patch=sin(p.x*0.7+sin(p.z*0.45))*sin(p.z*0.9+p.y*0.65);
                half fine=sin(p.x*7+p.y*5)*sin(p.z*8-p.y*6);
                clip(n.y+_UpBias+patch*0.22+fine*0.055-0.48);
                Light light=GetMainLight(TransformWorldToShadowCoord(p));
                half3 illumination=SampleSH(n)+light.color*saturate(dot(n,light.direction))*light.shadowAttenuation;
                half3 color=_SnowColor.rgb*(0.96+fine*0.04)*illumination;
                return half4(MixFog(color,input.fog),1);
            }
            ENDHLSL
        }
    }
}
