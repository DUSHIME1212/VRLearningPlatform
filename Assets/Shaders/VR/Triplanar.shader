// VRLearning/VR/Triplanar — world-space triplanar texturing (no UVs needed).
// Ideal for the AI/Meshy models with bad UVs. URP + single-pass-instanced (VR) safe.
Shader "VRLearning/VR/Triplanar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        _Tiling ("World Tiling", Float) = 1
        _Blend ("Blend Sharpness", Range(1,8)) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _BaseColor;
                float  _Tiling;
                half   _Blend;
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
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                half3 N = normalize(IN.normalWS);

                half3 bw = pow(abs(N), _Blend);
                bw /= max(bw.x + bw.y + bw.z, 1e-4);

                float3 wp = IN.positionWS * _Tiling;
                half3 cx = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, wp.zy).rgb;
                half3 cy = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, wp.xz).rgb;
                half3 cz = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, wp.xy).rgb;
                half3 albedo = (cx * bw.x + cy * bw.y + cz * bw.z) * _BaseColor.rgb;

                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(N, mainLight.direction)) * 0.75 + 0.25;
                half3 col = albedo * mainLight.color * ndotl;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
