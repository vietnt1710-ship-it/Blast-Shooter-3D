Shader "Custom/MySimpleSurface"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        //_TopColor ("Top Color", Color) = (1,1,1,1)
        //_BottomColor ("Bottom Color", Color) = (1,1,1,1)
        //_TopHeight ("Top Height", float) = 10
        //_BottomHeight ("Bototm Height", float) = 0
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EmissionMult ("Emission Multiple", float) = 0
        _Saturation ("Saturation", float) = 1.0
        _Brightness ("Brightness", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 200

        Cull Back
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #include "UnityCG.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldRefl;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        float _Saturation;
        float _Brightness;
        //fixed4 _Color;
        //fixed4 _BottomColor;
        //fixed4 _TopColor;
        //float _BottomHeight;
        //float _TopHeight;
        float _EmissionMult;

        UNITY_INSTANCING_BUFFER_START(props)
        UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
        UNITY_INSTANCING_BUFFER_END(props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //fixed4 col = lerp(_BottomColor, _TopColor, abs(IN.worldPos.y - _BottomHeight) / (_TopHeight - _BottomHeight));
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * col;
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(props, _Color);
            fixed lum = saturate(Luminance(c.rgb) * -_Brightness);
            o.Albedo = lerp(c.rgb, fixed3(lum,lum,lum), -_Saturation);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = c.rgb * _EmissionMult;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
