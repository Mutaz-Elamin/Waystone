Shader "Custom/GeneralTerrainShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        const static float epsilon = 1E-4;

        struct Input
        {
            float3 worldPos;
        };

        int regionsCount;
        fixed4 baseColours[8];
        float baseHeights[8];
        float baseBlends[8];

        float minHeight;
        float maxHeight;

        float inverseLerp(float start, float end, float givenValue) {

			return saturate((givenValue - start) / (end - start));
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            o.Albedo = baseColours[0];
			for (int i = 0; i < regionsCount; i ++) {
				float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - baseHeights[i]);
				o.Albedo = o.Albedo * (1-drawStrength) + baseColours[i] * drawStrength;
			}
        }
        ENDCG
    }
    FallBack "Diffuse"
}
