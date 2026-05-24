Shader "Hidden/SoftBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurDirection ("Blur Direction", Vector) = (1, 0, 0, 0)
        _BlurSize ("Blur Size", Float) = 0.35
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float2 _BlurDirection;
            float _BlurSize;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 offset = _BlurDirection * _BlurSize * _MainTex_TexelSize.xy;

                fixed4 color = tex2D(_MainTex, i.uv) * 0.4;
                color += tex2D(_MainTex, i.uv - offset * 2.0) * 0.15;
                color += tex2D(_MainTex, i.uv - offset) * 0.2;
                color += tex2D(_MainTex, i.uv + offset) * 0.2;
                color += tex2D(_MainTex, i.uv + offset * 2.0) * 0.15;

                return color;
            }
            ENDCG
        }
    }

    Fallback Off
}
