// VRLearning/VR/Dissolve — noise-threshold clip with emissive edge. For spawn/explode reveals.
// URP + single-pass-instanced (VR) safe.
Shader "VRLearning/VR/Dissolve"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        _NoiseTex ("Noise (R)", 2D) = "gray" {}
        _Amount ("Dissolve Amount", Range(0,1)) = 0.0
        [HDR] _EdgeColor ("Edge Color", Color) = (1,0.4,0.1,1)
        _EdgeWidth ("Edge Width", Range(0.001,0.3)) = 0.06
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }

        Pass
        {
            Name "Dissolve"
            Tags { "LightMode"="UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float2 uvNoise     : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);  SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _NoiseTex_ST;
                half4  _BaseColor;
                half   _Amount;
                half4  _EdgeColor;
                half   _EdgeWidth;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.uvNoise = TRANSFORM_TEX(IN.uv, _NoiseTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uvNoise).r;
                half edge = noise - _Amount;
                clip(edge - 0.0001);

                half3 N = normalize(IN.normalWS);
                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(N, mainLight.direction)) * 0.75 + 0.25;
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
                half3 lit = albedo * mainLight.color * ndotl;

                half glow = 1.0 - saturate(edge / _EdgeWidth);
                half3 col = lit + _EdgeColor.rgb * glow;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
