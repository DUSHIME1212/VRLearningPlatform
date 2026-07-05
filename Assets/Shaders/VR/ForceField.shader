// VRLearning/VR/ForceField — fresnel + scrolling pattern + camera-depth intersection glow.
// Needs Depth Texture enabled on the URP Renderer. URP + single-pass-instanced (VR) safe.
Shader "VRLearning/VR/ForceField"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.2,0.6,1,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
        _PatternTex ("Pattern", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Float) = 0.3
        [HDR] _IntersectColor ("Intersection Color", Color) = (1,1,1,1)
        _IntersectWidth ("Intersection Width", Float) = 0.4
        _Alpha ("Base Alpha", Range(0,1)) = 0.25
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

        Pass
        {
            Name "ForceField"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_PatternTex); SAMPLER(sampler_PatternTex);
            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                half   _RimPower;
                float4 _PatternTex_ST;
                float  _ScrollSpeed;
                half4  _IntersectColor;
                float  _IntersectWidth;
                half   _Alpha;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                VertexPositionInputs p = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = p.positionCS;
                OUT.positionWS  = p.positionWS;
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _PatternTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                half3 N = normalize(IN.normalWS);
                half3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));

                half fres = pow(1.0 - saturate(dot(N, V)), _RimPower);

                float2 uv = IN.uv + _Time.y * float2(_ScrollSpeed, _ScrollSpeed * 0.5);
                half pattern = SAMPLE_TEXTURE2D(_PatternTex, sampler_PatternTex, uv).r;

                // Soft intersection with scene geometry (needs Depth Texture on the URP asset)
                float2 screenUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                float sceneEye = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float thisEye  = -TransformWorldToView(IN.positionWS).z;
                half inter = 1.0 - saturate((sceneEye - thisEye) / max(_IntersectWidth, 1e-3));

                half3 col = _Color.rgb * (fres + pattern * 0.5) + _IntersectColor.rgb * inter;
                half a = saturate(_Alpha + fres + inter) * _Color.a;
                return half4(col, a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
