Shader "Custom/TerrainHeightColor"
{
    Properties
    {
        _HeightMin ("Min Height", Float) = 0
        _HeightMax ("Max Height", Float) = 10
        _SeabedColor("Seabed Color", Color) = (1, 0, 0, 1)
        _GrassColor ("Grass Color", Color) = (0,1,0,1)
        _SnowColor ("Snow Color", Color) = (1,1,1,1)
        _BlendSharpness ("Blend Sharpness", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Enable shadows on all light paths
        #pragma surface surf Standard fullforwardshadows

        // Define shader model
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
        };

        float _HeightMin;
        float _HeightMax;
        float4 _SeabedColor;
        float4 _GrassColor;
        float4 _SnowColor;
        float _BlendSharpness;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Get the height of the current point
            float height = IN.worldPos.y;

            // Normalize height between 0 and 1
            float t = saturate((height - _HeightMin) / (_HeightMax - _HeightMin));

            // Apply blend sharpness for smoother transitions
            t = pow(t, _BlendSharpness);

            // Interpolate between grass and snow colors
            float4 color;
            if (t < 0.5) {
                color = lerp(_SeabedColor, _GrassColor, t * 2);
            } else {
                color = lerp(_GrassColor, _SnowColor, (t - 0.5) * 2);
            }

            o.Albedo = color.rgb;
            o.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}