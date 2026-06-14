Shader "RailRevival/CloudSeaTrain"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Range(0, 10)) = 1.9
        _TimeScale ("Time Scale", Range(0.1, 10)) = 4.0
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

            sampler2D _NoiseTex;
            float _NoiseStrength;
            float _TimeScale;
            float2 _NoiseTexSize;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float noise(float2 x)
            {
                float2 f = frac(x);
                float2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
                float2 p = floor(x);
                float2 res = _NoiseTexSize;
                float a = tex2D(_NoiseTex, (p + float2(0.5, 0.5)) / res).x;
                float b = tex2D(_NoiseTex, (p + float2(1.5, 0.5)) / res).x;
                float c = tex2D(_NoiseTex, (p + float2(0.5, 1.5)) / res).x;
                float d = tex2D(_NoiseTex, (p + float2(1.5, 1.5)) / res).x;
                float value = a + (b - a) * u.x + (c - a) * u.y + (a - b - c + d) * u.x * u.y;
                return value * _NoiseStrength;
            }

            float fbm(float2 x, int detail)
            {
                float a = 0.0; float b = 1.0; float t = 0.0;
                for (int i = 0; i < detail; i++)
                {
                    float n = noise(x);
                    a += b * n; t += b; b *= 0.7; x *= 2.0;
                }
                return a / t;
            }

            float fbm2(float2 x, int detail)
            {
                float a = 0.0; float b = 1.0; float t = 0.0;
                for (int i = 0; i < detail; i++)
                {
                    float n = noise(x);
                    a += b * n; t += b; b *= 0.9; x *= 2.0;
                }
                return a / t;
            }

            #define LAYER(dh, v) if (uv.y < h + midlevel - (dh)) return float4(v, 1.0);

            float4 foreground_layer(float2 uv, float t)
            {
                float midlevel, h, disp, dist;
                float2 uv2;
                uv.y -= 0.2;
                midlevel = -0.1; disp = 1.7; dist = 1.0;
                uv2 = uv + float2(t/dist + 40.0, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.12, float3(0.43, 0.32, 0.31));
                LAYER(0.08, float3(0.55, 0.42, 0.41));
                LAYER(0.04, float3(0.66, 0.42, 0.40));
                LAYER(0.0, float3(0.77, 0.48, 0.46));
                midlevel = 0.05; disp = 1.7; dist = 2.0;
                uv2 = uv + float2(t/dist + 38.0, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.1, float3(0.95, 0.66, 0.48));
                LAYER(0.04, float3(0.98, 0.76, 0.64));
                LAYER(0.0, float3(0.95, 0.80, 0.77));
                return float4(0, 0, 0, 0);
            }

            float4 background_layer(float2 uv, float t)
            {
                float midlevel, h, disp, dist;
                float2 uv2;
                midlevel = 0.3; disp = 0.9; dist = 10.0;
                uv2 = uv + float2(t/dist + 32.5, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.14, float3(0.48, 0.19, 0.20));
                LAYER(0.1, float3(0.68, 0.28, 0.19));
                LAYER(0.07, float3(0.88, 0.38, 0.24));
                LAYER(0.0, float3(0.95, 0.45, 0.30));
                midlevel = 0.35; disp = 1.0; dist = 15.0;
                uv2 = uv + float2(t/dist + 30.0, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.04, float3(0.98, 0.76, 0.64));
                LAYER(0.0, float3(0.95, 0.80, 0.77));
                midlevel = 0.35; disp = 3.5; dist = 20.0;
                uv2 = uv + float2(t/dist + 27.5, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.12, float3(0.43, 0.32, 0.31));
                LAYER(0.08, float3(0.55, 0.42, 0.41));
                LAYER(0.04, float3(0.66, 0.42, 0.40));
                LAYER(0.0, float3(0.77, 0.48, 0.46));
                midlevel = 0.45; disp = 2.0; dist = 25.0;
                uv2 = uv + float2(t/dist + 23.0, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.04, float3(0.98, 0.57, 0.36));
                LAYER(0.0, float3(1.0, 0.62, 0.44));
                midlevel = 0.5; disp = 2.3; dist = 30.0;
                uv2 = uv + float2(t/dist + 20.5, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.12, float3(0.41, 0.27, 0.27));
                LAYER(0.08, float3(0.53, 0.35, 0.32));
                LAYER(0.04, float3(0.80, 0.24, 0.17));
                LAYER(0.0, float3(0.99, 0.29, 0.20));
                midlevel = 0.5; disp = 2.5; dist = 35.0;
                uv2 = uv + float2(t/dist + 18.0, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.1, float3(0.88, 0.38, 0.24));
                LAYER(0.05, float3(0.98, 0.42, 0.28));
                LAYER(0.0, float3(1.0, 0.48, 0.35));
                midlevel = 0.6; disp = 2.0; dist = 40.0;
                uv2 = uv + float2(t/dist + 18.0, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.1, float3(0.95, 0.66, 0.48));
                LAYER(0.0, float3(1.0, 0.76, 0.60));
                midlevel = 0.75; disp = 3.5; dist = 45.0;
                uv2 = uv + float2(t/dist + 15.5, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.2, float3(1.0, 0.55, 0.33));
                LAYER(0.15, float3(0.98, 0.50, 0.24));
                LAYER(0.1, float3(0.90, 0.55, 0.40));
                LAYER(0.0, float3(1.0, 0.62, 0.44));
                midlevel = 0.7; disp = 2.7; dist = 50.0;
                uv2 = uv + float2(t/dist + 12.0, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.04, float3(0.73, 0.36, 0.30));
                LAYER(0.0, float3(0.80, 0.40, 0.34));
                midlevel = 0.8; disp = 2.7; dist = 60.0;
                uv2 = uv + float2(t/dist + 9.5, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.1, float3(0.93, 0.58, 0.35));
                LAYER(0.0, float3(1.0, 0.76, 0.60));
                midlevel = 0.9; disp = 3.0; dist = 70.0;
                uv2 = uv + float2(t/dist + 7.0, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.1, float3(0.56, 0.25, 0.22));
                LAYER(0.05, float3(0.60, 0.30, 0.27));
                LAYER(0.0, float3(0.74, 0.35, 0.30));
                midlevel = 1.0; disp = 5.0; dist = 100.0;
                uv2 = uv + float2(t/dist + 3.5, 0.0);
                h = (fbm(uv2, 8) - 0.5) * disp;
                LAYER(0.1, float3(0.92, 0.85, 0.82));
                LAYER(0.0, float3(1.0, 0.94, 0.91));
                return float4(0.58, 0.7, 1.0, 1.0);
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.y = 1.0 - uv.y;
                float t = _Time.y * _TimeScale;

                float3 currentFrameColor = background_layer(uv, t).rgb;

                float2 trainUV = uv;
                trainUV.y -= 0.2;
                float2 uv2 = frac(trainUV * 9.0);

                float wagon = 1.0;
                wagon *= 1.0 - step(0.45, trainUV.x);
                wagon *= 1.0 - step(0.115, trainUV.y);
                wagon *= step(0.103, trainUV.y);
                wagon *= step(0.05, 1.0 - abs(uv2.x * 2.0 - 1.0));

                float join = 1.0;
                join *= 1.0 - step(0.45, trainUV.x);
                join *= 1.0 - step(0.11, trainUV.y);
                join *= step(0.107, trainUV.y);

                float roof = 1.0;
                roof *= 1.0 - step(0.45, trainUV.x);
                roof *= 1.0 - step(0.117, trainUV.y);
                roof *= step(0.11, trainUV.y);
                roof *= step(0.15, 1.0 - abs(uv2.x * 2.0 - 1.0));

                float loco = (trainUV.x > 0.45 && trainUV.x < 0.5 && trainUV.y > 0.103 && trainUV.y < 0.112) ? 1.0 : 0.0;
                float chem1 = (trainUV.x > 0.49 && trainUV.x < 0.495 && trainUV.y > 0.103 && trainUV.y < 0.12) ? 1.0 : 0.0;
                float chem2 = (trainUV.x > 0.488 && trainUV.x < 0.496 && trainUV.y > 0.12 && trainUV.y < 0.123) ? 1.0 : 0.0;
                float locoRoof = (trainUV.x > 0.443 && trainUV.x < 0.47 && trainUV.y > 0.11 && trainUV.y < 0.117) ? 1.0 : 0.0;

                float2 wp1 = trainUV - float2(0.457, 0.106);
                float2 wp2 = trainUV - float2(0.487, 0.105);
                float2 wp3 = trainUV - float2(0.497, 0.105);
                float wheel = 1.0 - step(0.00004, dot(wp1, wp1));
                wheel += 1.0 - step(0.00002, dot(wp2, wp2));
                wheel += 1.0 - step(0.00002, dot(wp3, wp3));
                if (trainUV.x < 0.45 && trainUV.y > 0.025 && trainUV.y < 0.2)
                {
                    wheel += 1.0 - step(0.002, dot(uv2 - float2(0.2, 0.95), uv2 - float2(0.2, 0.95)));
                    wheel += 1.0 - step(0.002, dot(uv2 - float2(0.8, 0.95), uv2 - float2(0.8, 0.95)));
                }

                currentFrameColor = lerp(currentFrameColor, float3(0.18, 0.12, 0.15), join);
                currentFrameColor = lerp(currentFrameColor, float3(0.48, 0.19, 0.20), wagon);
                currentFrameColor = lerp(currentFrameColor, float3(0.18, 0.12, 0.15), roof);
                currentFrameColor = lerp(currentFrameColor, float3(0.38, 0.19, 0.20), loco);
                currentFrameColor = lerp(currentFrameColor, float3(0.38, 0.19, 0.20), chem1);
                currentFrameColor = lerp(currentFrameColor, float3(0.18, 0.12, 0.15), locoRoof);
                currentFrameColor = lerp(currentFrameColor, float3(0.18, 0.12, 0.15), chem2 + wheel);

                float smokeDist = 5.0;
                float2 smokeUV = trainUV + float2(t/smokeDist + 3.5, 0.0);
                smokeUV.x -= t/smokeDist * 0.2;
                float smokeH = fbm2(smokeUV, 8) - 0.55;
                if (trainUV.x < 0.49)
                {
                    float sx = -trainUV.x + 0.49;
                    float sy = abs(trainUV.y + smokeH * 0.4 - 0.16 * sqrt(sx) - 0.12) - 0.8 * sx * exp(-sx * 10.0);
                    if (sy < 0.0) currentFrameColor = float3(1.0, 0.94, 0.91);
                    if (sy < -0.02) currentFrameColor = float3(0.92, 0.85, 0.82);
                }

                float2 bridgeUV = trainUV + float2(t/5.0 + 32.5, 0.0);
                bridgeUV.x = frac(bridgeUV.x * 3.0);
                float bk = 1.0;
                bk *= smoothstep(0.001, 0.003, abs(bridgeUV.y - pow(bridgeUV.x - 0.5, 2.0) * 0.15 - 0.12));
                bk *= min(step(0.05, 1.0 - abs(bridgeUV.x * 2.0 - 1.0)) + step(0.17, bridgeUV.y), 1.0);
                bk *= min(smoothstep(0.02, 0.05, 1.0 - abs(bridgeUV.x * 2.0 - 1.0)) + step(0.177, bridgeUV.y), 1.0);
                bk *= min(step(0.1, bridgeUV.y) + smoothstep(-0.09, -0.085, -bridgeUV.y - 0.001 / (1.0 - abs(bridgeUV.x * 2.0 - 1.0))), 1.0);
                bk *= min(smoothstep(0.05, 0.2, 1.0 - abs(frac(bridgeUV.x * 16.0) * 2.0 - 1.0)) + step(0.12, bridgeUV.y - pow(bridgeUV.x - 0.5, 2.0) * 0.15) + step(-0.1, -bridgeUV.y), 1.0);
                currentFrameColor = lerp(float3(0.29, 0.09, 0.08) * smoothstep(-0.08, 0.08, trainUV.y), currentFrameColor, bk);

                float3 fgAccum = float3(0, 0, 0);
                float fgAlpha = 0.0;
                for (int fi = 0; fi < 5; fi++)
                {
                    float4 fgSample = foreground_layer(uv, t + 4.0 * float(fi) / 5.0 / 60.0);
                    if (fgSample.a > 0.0)
                    {
                        fgAccum += fgSample.rgb;
                        fgAlpha += 1.0;
                    }
                }
                if (fgAlpha > 0.0)
                {
                    fgAccum /= fgAlpha;
                    fgAlpha = clamp(fgAlpha / 5.0, 0.0, 1.0);
                    currentFrameColor = lerp(currentFrameColor, fgAccum, fgAlpha);
                }

                float vignette = 16.0 * i.uv.x * i.uv.y * (1.0 - i.uv.x) * (1.0 - i.uv.y);
                float vignetteFactor = 0.5 + 0.5 * pow(vignette, 0.25);

                return float4(currentFrameColor * vignetteFactor, 1.0);
            }
            ENDCG
        }
    }
}
