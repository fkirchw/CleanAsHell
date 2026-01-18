Shader "Custom/TilemapBlood"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BloodMask ("Blood Mask", 2D) = "black" {}
        _BloodTexture ("Blood Splatter Pattern", 2D) = "white" {}
        _BloodColor ("Blood Tint", Color) = (0.5, 0, 0, 1)
        _BloodTiling ("Blood Pattern Tiling", Float) = 2.0
        _CellSize ("Cell Size", Vector) = (1, 1, 0, 0)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BloodMask;
            sampler2D _BloodTexture;

            float4 _BloodColor;
            float _BloodTiling;
            float4 _MainTex_ST;
            float4 _BloodMask_ST;
            float2 _CellSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;

                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, i.texcoord);

                // Blood mask UV
                float2 bloodMaskUV = (i.worldPos - _BloodMask_ST.zw) * _BloodMask_ST.xy;

                // Clamp to valid range with a tiny epsilon to avoid edge bleeding
                bloodMaskUV = clamp(bloodMaskUV, 0.001, 0.999);

                fixed bloodAmount = tex2D(_BloodMask, bloodMaskUV).r;

                if (bloodAmount < 0.001)
                {
                    return baseColor * i.color;
                }

                // Use cell-relative coordinates for pattern sampling
                // Divide world pos by cell size to get cell-space coords
                float2 cellSpacePos = i.worldPos / _CellSize;

                // Sample blood pattern using cell-space coordinates
                fixed4 bloodPattern = tex2D(_BloodTexture, cellSpacePos * _BloodTiling);

                fixed4 darkenedBase = baseColor * (1.0 - bloodAmount * 0.15);
                fixed4 bloodOverlay = bloodPattern * _BloodColor;
                fixed4 finalColor = darkenedBase + (bloodOverlay * bloodAmount * 2.0);
                finalColor *= i.color;
                finalColor.a = baseColor.a;

                return finalColor;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}