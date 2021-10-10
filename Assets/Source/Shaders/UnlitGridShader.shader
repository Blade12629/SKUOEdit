Shader "CustomShaders/UnlitGridShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _DrawGrid("DrawGrid", int) = 0
        _GridSize("GridSize", float) = 0.05
        _GridColor("GridColor", COLOR) = (1, 0, 0, 1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : float;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _DrawGrid;
            float _GridSize;
            float4 _GridColor;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                //o.worldPos = o.vertex;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_DrawGrid > 0)
                {
                    float x = i.worldPos.x - (int)i.worldPos.x;
                    float z = i.worldPos.z - (int)i.worldPos.z;

                    //if (x < 0)
                    //    x = -x;

                    //if (z < 0)
                    //    z = -z;

                    if (x < _GridSize || x > 1 - _GridSize ||
                        z < _GridSize || z > 1 - _GridSize)
                    {
                        return _GridColor;
                    }
                }

                // sample the texture
                return tex2D(_MainTex, i.uv);
            }
        ENDCG
    }
    }
}
