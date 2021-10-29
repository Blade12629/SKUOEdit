Shader "CustomShaders/StaticShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}

        // Selected Tile
        _SelectedColor("SelectedColor", COLOR) = (1, 0, 0, 1)
        _EnableSelectedRendering("EnableSelectedRendering", int) = 0
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

            // Selected Tile
            float4 _SelectedColor;
            int _EnableSelectedRendering;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float wxmod = i.worldPos.x - (int)i.worldPos.x;
                float wzmod = i.worldPos.z - (int)i.worldPos.z;
                float wymod = i.worldPos.y - (int)i.worldPos.y;

                if (_EnableSelectedRendering)
                {
                    if (wxmod < .04f || wxmod > .96f ||
                        wzmod < .04f || wzmod > .96f)
                    {
                        return _SelectedColor;
                    }
                }

                return tex2D(_MainTex, i.uv);
            }
        ENDCG
    }
    }
}
