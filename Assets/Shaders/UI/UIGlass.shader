// Glassmorphism look for UGUI panels: a translucent base (via the Image's own Color/alpha)
// plus a vertical "sheen" gradient and a soft edge highlight, all computed in the fragment
// shader — no grab-pass/blur (too costly for standalone VR), just cheap alpha compositing.
// Built on Unity's standard UI/Default shader structure so stencil masking and RectMask2D
// clipping still work normally inside any Canvas.
Shader "UI/Glass"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        _EdgeColor ("Edge Highlight Color", Color) = (1,1,1,0.5)
        _EdgeWidth ("Edge Width", Range(0.001, 0.3)) = 0.05
        _TopColor ("Sheen Top Color", Color) = (1,1,1,0.18)
        _BottomColor ("Sheen Bottom Color", Color) = (1,1,1,0.02)

        // Aspect-correct rounded-rect mask. _RectSize is pushed per-instance (in rect units, e.g.
        // pixels) by UIGlassRounder.cs so corners stay circular regardless of the panel's own W:H
        // ratio — raw UI shaders have no built-in access to the RectTransform's actual size.
        _CornerRadiusPx ("Corner Radius (rect units)", Float) = 24
        _RectSize ("Rect Size (rect units, w/h) — pushed by UIGlassRounder", Vector) = (100, 100, 0, 0)

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex         : SV_POSITION;
                fixed4 color          : COLOR;
                float2 texcoord       : TEXCOORD0;
                float4 worldPosition  : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _EdgeColor;
            float _EdgeWidth;
            fixed4 _TopColor;
            fixed4 _BottomColor;

            float _CornerRadiusPx;
            float4 _RectSize;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                // Vertical sheen: lighter near the top, fading out toward the bottom.
                fixed4 sheen = lerp(_BottomColor, _TopColor, IN.texcoord.y);
                color.rgb = lerp(color.rgb, sheen.rgb, sheen.a);
                color.a = saturate(color.a + sheen.a * 0.5);

                // Soft bright edge on all four sides — the "glass rim" look.
                float distToEdge = min(min(IN.texcoord.x, 1 - IN.texcoord.x), min(IN.texcoord.y, 1 - IN.texcoord.y));
                float edge = 1 - smoothstep(0, _EdgeWidth, distToEdge);
                color.rgb = lerp(color.rgb, _EdgeColor.rgb, edge * _EdgeColor.a);
                color.a = saturate(color.a + edge * _EdgeColor.a * 0.5);

                // Aspect-ratio-correct rounded-rect mask (IQ round-box SDF), evaluated in the same
                // rect-unit space _RectSize is fed in, so corners stay circular regardless of the
                // panel's own W:H ratio. Multiplying color.a here (before clip-rect/alpha-clip below)
                // means rounding composes correctly with RectMask2D — final visible area is the
                // intersection of both.
                float2 halfSize = _RectSize.xy * 0.5;
                float2 p        = (IN.texcoord - 0.5) * _RectSize.xy;
                float2 q        = abs(p) - halfSize + _CornerRadiusPx;
                float  roundDist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - _CornerRadiusPx;
                color.a *= 1 - smoothstep(0.0, 1.5, roundDist);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
