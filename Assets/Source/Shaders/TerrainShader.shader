Shader "CustomShaders/TerrainShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}

        // Grid
        _DrawGrid("DrawGrid", int) = 0
        _GridSize("GridSize", float) = 0.05
        _GridColor("GridColor", COLOR) = (1, 0, 0, 1)

        // Selected Tile
        _SelectedPos("SelectedPos", VECTOR) = (0, 0, 0)
        _SelectedAreaSize("SelectedAreaSize", int) = 1
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

            // Grid
            int _DrawGrid;
            float _GridSize;
            float4 _GridColor;
            
            // Selected Tile
            float4 _SelectedPos;
            int _SelectedAreaSize;
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
                int result = 0;
                float wxmod = i.worldPos.x - (int)i.worldPos.x;
                float wzmod = i.worldPos.z - (int)i.worldPos.z;

                // Grid
                if (_DrawGrid > 0)
                {
                    if (wxmod < _GridSize || wxmod > 1 - _GridSize ||
                        wzmod < _GridSize || wzmod > 1 - _GridSize)
                    {
                        result = 1;
                    }
                }

                // Selected Tile
                // for some reason this does not work correctly, maybe someone who is better at this could help me out here (pretty easy to see what i'm talking about once you have loaded any map in
                                                                                                                          // and move your cursor around
                if (_EnableSelectedRendering)
                {
                    float xMin = (int)_SelectedPos.x - _SelectedAreaSize;
                    float xMax = (int)_SelectedPos.x + _SelectedAreaSize;

                    if (xMin <= i.worldPos.x && xMax >= i.worldPos.x)
                    {
                        float zMin = (int)_SelectedPos.z - _SelectedAreaSize;
                        float zMax = (int)_SelectedPos.z + _SelectedAreaSize;

                        if (zMin <= i.worldPos.z && zMax >= i.worldPos.z &&
                            (wxmod < _GridSize || wxmod > 1 - _GridSize ||
                                wzmod < _GridSize || wzmod > 1 - _GridSize))
                        {
                            result = 2;
                        }
                    }
                }

                switch (result)
                {
                    default:
                    case 0:
                        // sample the texture
                        return tex2D(_MainTex, i.uv);

                    case 1:
                        return _GridColor;

                    case 2:
                        return _SelectedColor;
                }
            }
        ENDCG
    }
    }
}
