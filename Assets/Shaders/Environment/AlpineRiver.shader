Shader "TopGun/Alpine/FlowingRiver"
{
    Properties
    {
        _ShallowColor ("Shallow water", Color) = (0.12,0.34,0.32,1)
        _DeepColor ("Deep water", Color) = (0.025,0.095,0.12,1)
        _FoamColor ("Foam", Color) = (0.72,0.82,0.8,1)
        _FlowSpeed ("Flow speed (meters/sec)", Range(0,12)) = 4
        _FlowDirection ("Flow direction (world XZ)", Vector) = (0,-1,0,0)
        _WaveStrength ("Wave strength", Range(0,0.4)) = 0.12
        _DepthFade ("Depth color distance", Range(0.1,12)) = 4
        _FoamWidth ("Shore foam width", Range(0.05,3)) = 0.75
        _Reflection ("Reflection strength", Range(0,1)) = 0.45
        _Opacity ("Water opacity", Range(0,1)) = 0.55
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Name "RiverForward"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _ShallowColor, _DeepColor, _FoamColor;
            float4 _FlowDirection;
            float _FlowSpeed, _WaveStrength, _DepthFade, _FoamWidth, _Reflection, _Opacity;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float2 uv:TEXCOORD1; half fog:TEXCOORD2; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS;
                o.uv=v.uv; o.fog=ComputeFogFactor(p.positionCS.z); return o;
            }
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p)
            {
                float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            half4 Frag(Varyings i):SV_Target
            {
                float2 screenUV=GetNormalizedScreenSpaceUV(i.positionCS);
                float rawDepth=SampleSceneDepth(screenUV);
                float sceneDepth=LinearEyeDepth(rawDepth,_ZBufferParams);
                float surfaceDepth=-TransformWorldToView(i.positionWS).z;
                float depth=max(0,sceneDepth-surfaceDepth)*max(0.1,abs(GetWorldSpaceNormalizeViewDir(i.positionWS).y));
                float t=_Time.y*_FlowSpeed;
                // Sample against the flow vector so visible ripples travel downstream.
                float2 direction=_FlowDirection.xy/max(length(_FlowDirection.xy),0.001);
                float2 flow=(i.positionWS.xz-direction*t)*float2(.22,.65);
                float ripple=Noise(flow*float2(.8,1.8))+Noise(flow*float2(1.9,3.1)+float2(t*.3,7))*.45;
                float2 waveUV=flow*float2(.55,.8); float nx=(Noise(waveUV+float2(.25,0))-Noise(waveUV-float2(.25,0)))*3;
                float nz=(Noise(waveUV+float2(0,.25))-Noise(waveUV-float2(0,.25)))*3; float filter=1/(1+length(fwidth(flow))*2); nx*=filter; nz*=filter;
                half3 n=normalize(half3(nx*_WaveStrength,1,nz*_WaveStrength));
                half3 view=GetWorldSpaceNormalizeViewDir(i.positionWS);
                half fresnel=0.04+0.96*pow(1-saturate(dot(n,view)),5);
                Light light=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 water=lerp(_ShallowColor.rgb,_DeepColor.rgb,1-exp(-depth/_DepthFade));
                water*=SampleSH(half3(0,1,0))+light.color*(0.25+0.45*saturate(dot(n,light.direction)))*light.shadowAttenuation;
                half3 reflection=GlossyEnvironmentReflection(reflect(-view,n),0.16,1);
                water=lerp(water,reflection,saturate(fresnel*_Reflection));
                half specular=pow(saturate(dot(n,normalize(view+light.direction))),180)*0.45;
                water+=specular*light.color*light.shadowAttenuation;
                float edge=min(i.uv.x,1-i.uv.x);
                float shoreline=(1-smoothstep(0.008,0.07,edge))*(0.4+0.6*(1-saturate(depth/_FoamWidth)));
                float foam=shoreline*smoothstep(.48,.85,ripple);
                float streak=smoothstep(1.08,1.38,ripple)*.12;
                water=lerp(water,_FoamColor.rgb,saturate(foam*.7+streak));
                float alpha=saturate((.36+(1-exp(-depth*.45))*.42+fresnel*.2)*_Opacity+foam*.24)*smoothstep(0,.025,edge);
                return half4(MixFog(water,i.fog),alpha);
            }
            ENDHLSL
        }
    }
}



