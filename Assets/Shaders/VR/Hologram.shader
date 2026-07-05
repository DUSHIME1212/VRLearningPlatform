// VRLearning/VR/Hologram — fresnel + animated scanlines + flicker, transparent additive.
// URP + single-pass-instanced (VR) safe.
Shader "VRLearning/VR/Hologram"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.2,0.8,1,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 2
        _ScanTiling ("Scanline Tiling", Float) = 60
        _ScanSpeed ("Scan Speed", Float) = 3
        _ScanIntensity ("Scan Intensity", Range(0,1)) = 0.5
        _FlickerSpeed ("Flicker Speed", Float) = 10
        _Alpha ("Base Alpha", Range(0,1)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }

        Pass
        {
            Name "Hologram"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            CBUFFER_START(UnityPerMaterial)
                half4  _Color;
                half   _RimPower;
                float  _ScanTiling;
                float  _ScanSpeed;
                half   _ScanIntensity;
                float  _FlickerSpeed;
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
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                half3 N = normalize(IN.normalWS);
                half3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));

                half fres = pow(1.0 - saturate(dot(N, V)), _RimPower);
                half scan = sin(IN.positionWS.y * _ScanTiling - _Time.y * _ScanSpeed) * 0.5 + 0.5;
                scan = lerp(1.0, scan, _ScanIntensity);
                half flick = 0.85 + 0.15 * sin(_Time.y * _FlickerSpeed);

                half3 col = _Color.rgb * (fres + 0.25) * scan * flick;
                half a = saturate((_Alpha + fres) * scan) * flick * _Color.a;
                return half4(col, a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
