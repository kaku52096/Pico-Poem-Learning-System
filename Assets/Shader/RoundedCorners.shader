Shader "UI/RoundedCorners"
{
    Properties
    {
        [HideInInspector]_Radius ("Radius", Range(0, 0.5)) = 0.1
        [HideInInspector]_Ratio ("Height/Width", float) = 1

        //MASK SUPPORT ADD
        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask("Color Mask", Float) = 15
        //MASK SUPPORT END
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Cull Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha

        //MASK SUPPORT ADD
        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        ColorMask[_ColorMask]
        //MASK SUPPORT END

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float _Radius;
            float _Ratio;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {  
                float2 p = abs(step(0.5, i.uv) - i.uv);
                fixed4 col = i.color * (step(_Radius, p.x) || step(_Radius, p.y*_Ratio) || step(length(float2(p.x-_Radius, p.y*_Ratio-_Radius)), _Radius));
                return col;
            }
            ENDCG
        }
    }
}