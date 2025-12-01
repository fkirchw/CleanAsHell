Shader "Custom/TilemapBlood"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BloodMask ("Blood Mask", 2D) = "black" {}
        _BloodTexture ("Blood Splatter Pattern", 2D) = "white" {}
        _BloodColor ("Blood Tint", Color) = (0.5, 0, 0, 1)
        _BloodTiling ("Blood Pattern Tiling", Float) = 2.0
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

            // Texture samplers
            sampler2D _MainTex;
            sampler2D _BloodMask;
            sampler2D _BloodTexture;
            
            // Properties
            float4 _BloodColor;
            float _BloodTiling;
            float4 _MainTex_ST;
            float4 _BloodMask_ST;

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
                float2 worldPos : TEXCOORD1;  // Add world position
            };

            v2f vert(appdata v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                
                // Calculate world position for blood mask sampling
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                
                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the base tile texture
                fixed4 baseColor = tex2D(_MainTex, i.texcoord);
                
                // Sample the blood mask using WORLD POSITION, not sprite UVs
                // This ensures each tile samples its corresponding position in the mask
                float2 bloodMaskUV = i.worldPos * _BloodMask_ST.xy + _BloodMask_ST.zw;
                fixed bloodAmount = tex2D(_BloodMask, bloodMaskUV).r;
                
                // Early exit if no blood
                if (bloodAmount < 0.001)
                {
                    return baseColor * i.color;
                }
                
                // Sample blood splatter pattern with tiling
                fixed4 bloodPattern = tex2D(_BloodTexture, i.texcoord * _BloodTiling);
                
                // Darken the base tile where blood is (blood soaks in)
                fixed4 darkenedBase = baseColor * (1.0 - bloodAmount * 0.3);
                
                // Create blood overlay color
                fixed4 bloodOverlay = bloodPattern * _BloodColor;
                
                // Combine: darkened base + blood on top
                fixed4 finalColor = darkenedBase + (bloodOverlay * bloodAmount);
                
                // Apply vertex color
                finalColor *= i.color;
                
                // Preserve original alpha
                finalColor.a = baseColor.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}