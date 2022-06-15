Shader "CustomShaders/UltimaTerrainShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ShouldRenderGrid("Should Render Grid", int) = 1
        _GridSize("Grid Size", float) = 0.05
        _GridColor("Grid Color", COLOR) = (1, 0, 0, 1)
        _ShouldRenderSelection("Should Render Selection", int) = 1
        _SelectionPosition("Selection Position", VECTOR) = (0, 0, 0)
        _SelectionSize("Selection Size", int) = 3
        _SelectionColor("Selection Color", COLOR) = (0, 1, 0, 1)
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

            int _ShouldRenderGrid;
            float _GridSize;
            float4 _GridColor;

            int _ShouldRenderSelection;
            float3 _SelectionPosition;
            int _SelectionSize;
            float4 _SelectionColor;

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
                float xmod = i.worldPos.x - (int)i.worldPos.x;
                float ymod = i.worldPos.y - (int)i.worldPos.y;
                float gridMin = 1 - _GridSize;
                int retVal = 0;

                if (_ShouldRenderGrid != 0)
                {
                    if (_ShouldRenderSelection)
                    {
                        float x = i.worldPos.x - _SelectionPosition.x;
                        float y = i.worldPos.y - _SelectionPosition.y;

                        if (x >= 0 && x < _SelectionSize && y >= 0 && y < _SelectionSize)
                            retVal = 2;
                    }

                    if ((xmod <= _GridSize || xmod >= gridMin) ||
                        (ymod <= _GridSize || ymod >= gridMin))
                    {
                        if (retVal == 0)
                            retVal = 1;
                    }
                    else
                        retVal = 0;
                }

                switch (retVal)
                {
                    case 0:
                        break;
                    case 1:
                        return _GridColor;
                    case 2:
                        return _SelectionColor;
                }
                
                return tex2D(_MainTex, i.uv);
            }
        ENDCG
        }
    }
}
