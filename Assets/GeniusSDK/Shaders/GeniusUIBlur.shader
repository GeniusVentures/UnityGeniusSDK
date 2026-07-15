// Two-pass separable Gaussian, run through Graphics.Blit over a captured screenshot.
//
// NOT a GrabPass shader, deliberately. The game's canvases are Screen Space - Overlay, which
// is composited after every camera has finished; a GrabPass inside an Overlay canvas has
// nothing meaningful to grab and renders as garbage or black on most mobile GPUs. The screen
// is therefore captured explicitly (GeniusUIBlurBackdrop) and blurred here as a plain
// texture, which works regardless of canvas render mode.
//
// Separable: a 9-tap horizontal pass followed by a 9-tap vertical pass gives the same result
// as an 81-tap 2D kernel for 18 samples instead of 81. It runs once when the modal opens,
// not per frame.
Shader "UI/GeniusUIBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Size ("Blur Radius", Range(0, 8)) = 3
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Blend Off

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        float _Size;

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv  : TEXCOORD0;
        };

        v2f vert(appdata_img v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            return o;
        }

        // Normalised Gaussian weights for a 9-tap kernel (sigma ~= 2).
        static const float WEIGHTS[5] = { 0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162 };

        fixed4 Blur(float2 uv, float2 direction)
        {
            fixed4 sum = tex2D(_MainTex, uv) * WEIGHTS[0];

            [unroll]
            for (int i = 1; i < 5; i++)
            {
                float2 offset = direction * i * _Size;
                sum += tex2D(_MainTex, uv + offset) * WEIGHTS[i];
                sum += tex2D(_MainTex, uv - offset) * WEIGHTS[i];
            }

            sum.a = 1;
            return sum;
        }
        ENDCG

        // Pass 0 -- horizontal
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            fixed4 frag(v2f i) : SV_Target
            {
                return Blur(i.uv, float2(_MainTex_TexelSize.x, 0));
            }
            ENDCG
        }

        // Pass 1 -- vertical
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            fixed4 frag(v2f i) : SV_Target
            {
                return Blur(i.uv, float2(0, _MainTex_TexelSize.y));
            }
            ENDCG
        }
    }

    Fallback Off
}
