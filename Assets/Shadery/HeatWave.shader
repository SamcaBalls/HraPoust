Shader "Custom/HeatWave"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _DistortStrength ("Distortion Strength", Range(0,0.1)) = 0.02
        _Tint ("Heat Tint", Color) = (1,0.8,0.5,1)
        _Speed ("Wave Speed", Range(0,5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DistortStrength;
            float4 _Tint;
            float _Speed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
{
    float2 uv = i.uv;
    uv.y += sin((uv.x + _Time.y * _Speed) * 10) * _DistortStrength;
    uv.x += cos((uv.y + _Time.y * _Speed) * 10) * _DistortStrength;

    float4 col = tex2D(_MainTex, uv);
    col.rgb = lerp(col.rgb, _Tint.rgb, 0.2); // teplý odstín
    return col;
}
            ENDCG
        }
    }
}
