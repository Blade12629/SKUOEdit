Shader "Unlit/UnlitMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SelectedPosition("SelectedPosition", VECTOR) = (0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            float4 _SelectedPosition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float range = 1;
                float xmin = _SelectedPosition.x - range;
                float xmax = _SelectedPosition.x + range;
                float ymin = _SelectedPosition.y - range;
                float ymax = _SelectedPosition.y + range;
                float zmin = _SelectedPosition.z - range;
                float zmax = _SelectedPosition.z + range;


                if (i.vertex.x >= xmin && i.vertex.x <= xmax &&
                    i.vertex.y >= ymin && i.vertex.y <= ymax)
                {
                    col = float4(1, 0, 0, 1);
                }
                
                return col;
            }
            ENDCG
        }
    }
}
