Shader "TopGun/Effects/JetHeatHaze"
{
    Properties
    {
        _Distortion ("Distortion strength", Range(0,0.02)) = 0.004
        _Opacity ("Blend strength", Range(0,1)) = 0.45
        _NoiseScale ("Heat turbulence", Float) = 9
        _FlowSpeed ("Turbulence speed", Float) = 3
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+50" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Distortion, _Opacity, _NoiseScale, _FlowSpeed;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float3 positionWS:TEXCOORD1; half4 color:COLOR; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionWS=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                o.uv=v.uv; o.color=v.color; return o;
            }
            float Noise(float2 p)
            {
                return sin(p.x*1.7+sin(p.y*2.3))*sin(p.y*1.3+cos(p.x*2.1));
            }
            half4 Frag(Varyings i):SV_Target
            {
                float2 uv=GetNormalizedScreenSpaceUV(i.positionCS);
                float2 local=i.uv*2-1;
                float mask=pow(saturate(1-dot(local,local)),2);
                float eye=-TransformWorldToView(i.positionWS).z;
                float scene=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams);
                float fade=saturate((scene-eye)*2)*saturate((eye-0.3)*2);
                float2 p=i.uv*_NoiseScale+float2(_Time.y*_FlowSpeed,-_Time.y*_FlowSpeed*1.7);
                float2 offset=float2(Noise(p),Noise(p+13.4))*_Distortion*mask;
                offset.x*=_ScreenParams.y/_ScreenParams.x;
                float2 displaced=saturate(uv+offset);
                float displacedDepth=LinearEyeDepth(SampleSceneDepth(displaced),_ZBufferParams);
                displaced=lerp(uv,displaced,step(eye,displacedDepth));
                return half4(SampleSceneColor(displaced),mask*fade*_Opacity*i.color.a);
            }
            ENDHLSL
        }
    }
}