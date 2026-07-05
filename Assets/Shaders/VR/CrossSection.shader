// VRLearning/VR/CrossSection — clip a world-space plane and cap the interior with a color.
// Two-sided by design so you see inside organs. URP + single-pass-instanced (VR) safe.
Shader "VRLearning/VR/CrossSection"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (0.8,0.8,0.8,1)
        _PlanePosition ("Plane Position (World)", Vector) = (0,0,0,0)
        _PlaneNormal ("Plane Normal (World)", Vector) = (0,1,0,0)
        [HDR] _CrossColor ("Cross-Section Color", Color) = (1,0.2,0.2,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "CrossSection"
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
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                float4 _PlanePosition;
                float4 _PlaneNormal;
                half4  _CrossColor;
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
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag (Varyings IN, half facing : VFACE) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                // Discard everything on the +normal side of the plane.
                float3 pn = normalize(_PlaneNormal.xyz);
                float d = dot(IN.positionWS - _PlanePosition.xyz, pn);
                clip(-d);

                half3 N = normalize(IN.normalWS) * (facing >= 0 ? 1.0 : -1.0);
                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(N, mainLight.direction)) * 0.75 + 0.25;

                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb * _BaseColor.rgb;
                half3 col = albedo * mainLight.color * ndotl;

                // Interior (back) faces exposed by the cut get the cross-section color.
                if (facing < 0)
                    col = _CrossColor.rgb * (ndotl * 0.5 + 0.5);

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
