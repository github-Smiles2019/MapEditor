Shader "Bycw/WaterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // Wave
        waveVisuals("waveVisuals", Vector) = (0.167, 7, 0.54, 1)
        waveDirections("waveDirections", Vector) = (0, 0.3, 0.6, 0.67)
        // end

        // form
        foamParams("foamParams", Vector) = (0.35, 0.5, 1, 0.5)
        foamColor("foamColor", Color) = (1, 1, 1, 1)
        // end

        // caustic
        causticParams1("causticParams1", Vector) = (0.98, 2.33, 0.1, 0.35)
        causticDepth("causticDepth", Float) = 1.0
        causticColor("causticColor", Color) = (0, 0.313, 0.373)
        causticTexture("causticTexture", 2D) = "white" {}
        // end

        // depth
        surfaceWaterDepth("surfaceWaterDepth", 2D) = "white" {}
        depthGradientShallow("depthGradientShallow", Color) = (0.325, 0.807, 0.971, 0.725)
        depthGradientDeep("depthGradientDeep", Color) = (0.086, 0.407, 1, 0.749)
        depthMaxDistance("depthMaxDistance", Float) = 1.0
        // end

        //
        depthDistance("depthDistance", Float) = 1.0
        shallowColor("shallowColor", Color) = (1.0, 1.0, 1.0, 1.0)
        deepColor("deepColor", Color) = (0, 0, 0, 0)
        opacity("opacity", Float) = 1.0
        coastOpacity("coastOpacity", Float) = 0
        // 
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
            // make fog work
            #pragma multi_compile_fog

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
                float3 v_position: TEXCOORD1;
                float3 v_normal: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 waveVisuals;
            float4 waveDirections;

            float4 foamParams;
            float4 foamColor;
            float4 causticParams1;
            float causticDepth;
            float3 causticColor;

            sampler2D causticTexture;
            float4 causticTexture_ST;

            sampler2D surfaceWaterDepth;
            float4 surfaceWaterDepth_ST;

            float4 depthGradientShallow;
            float4 depthGradientDeep;
            float depthMaxDistance;

            float depthDistance;
            float4 shallowColor;
            float4 deepColor;
            float opacity;
            float coastOpacity;

            float3 gerstner(float3 position, float steepness, float wavelength, float speed, float direction, inout float3 tangent, inout float3 binormal)
            {
                float pi = 3.1415926;
                direction = direction * 2. - 1.;
                float2 d = normalize(float2(cos(pi * direction), sin(pi * direction)));
                float s = steepness;
                float k = 2. * pi / wavelength;                                                      
                float f = k * (dot(d, position.xz) - speed * _Time.y);
                float a = s / k;

                tangent += float3(
                -d.x * d.x * s * sin(f),
                d.x * s * cos(f), 
                -d.x * d.y * s * sin(f)
                );

                binormal += float3(
                -d.x * d.y * s * sin(f),
                d.y * s * cos(f),
                -d.y * d.y * s * sin(f)
                );

                return float3(
                d.x * a * cos(f),
                a * sin(f),
                d.y * a * cos(f)
                );
            }
            void gerstnerWaves(float3 p, float3 visuals, float4 directions, out float3 offset, out float3 normal, out float3 T, out float3 B)
            {
                float steepness = visuals.x;
                float wavelength = visuals.y;
                float speed = visuals.z;

                offset = float3(0,0,0);
                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);

                offset += gerstner(p, steepness, wavelength, speed, directions.x, tangent, binormal);
                offset += gerstner(p, steepness, wavelength, speed, directions.y, tangent, binormal);
                offset += gerstner(p, steepness, wavelength, speed, directions.z, tangent, binormal);
                offset += gerstner(p, steepness, wavelength, speed, directions.w, tangent, binormal);

                normal = normalize(cross(binormal, tangent));
                T = tangent;
                B = binormal;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                // float3 offset;
                // float3 tangent;
                // float3 bitangent;

                // gerstnerWaves(worldPos.xyz, waveVisuals.xyz, waveDirections, offset, o.v_normal, tangent, bitangent);
                // worldPos.xyz += offset;
                o.v_position = worldPos.xyz;
                

                return o;
            }

            float2 panner(float2 uv, float direction, float speed, float2 offset, float tiling)
            {
                float pi = 3.1415926;
                direction = direction * 2. - 1.;
                float2 dir = normalize(float2(cos(pi * direction), sin(pi * direction)));
                return  (dir * _Time.y * speed) + offset + (uv * tiling);
            }

            float3 rgbSplit(float split, sampler2D tex, float2 uv)
            {
                float2 UVR = uv + float2(split, split);
                float2 UVG = uv + float2(split, -split);
                float2 UVB = uv + float2(-split, -split);

                float r = tex2D(tex, UVR).r;
                float g = tex2D(tex, UVG).g;
                float b = tex2D(tex, UVB).b;

                return float3(r,g,b);
            }

            float3 caustic(float3 v_position)
            {
                float2 uv = v_position.xz;

                float strength = causticParams1.x;
                float split = causticParams1.w * 0.01;
                float speed = causticParams1.z;
                float scale = causticParams1.y;

                float3 texture1 = rgbSplit(split, causticTexture, panner(uv, 1.0, speed, float2(0., 0.), 1./scale));
                float3 texture2 = rgbSplit(split, causticTexture, panner(uv, 1.0, speed, float2(0., 0.), -1./scale));
                float3 textureCombined = min(texture1, texture2);

                return strength * 10.0 * textureCombined;
            }

            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1.0 - top.a));
                float alpha = top.a + bottom.a * (1.0 - top.a);
                return float4(color, alpha);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 waterColor = shallowColor;
                float4 finalFoamColor = float4(0, 0, 0, 0);
                float4 finalCausticColor = float4(0, 0, 0, 0);
                
                finalCausticColor.rgb = caustic(i.v_position) * causticColor;

                float waterDepth = tex2D(surfaceWaterDepth, i.uv).r;
                float depth = clamp(1. - waterDepth / depthMaxDistance, 0.0, 1.0);
                float4 depthColor = lerp(depthGradientShallow, depthGradientDeep, depth);
                waterColor = alphaBlend(depthColor, waterColor);
                float4 finalColor = waterColor + finalFoamColor + finalCausticColor;
                
                return finalColor;
            }
            ENDCG
        }
    }
}
