Shader "Unlit/MirrorWater"
{
    Properties
    {
        _MainTex ("Reflection", 2D) = "white" {}
        _Alpha ("Alpha", Range(0, 1)) = 1
        
        // --- 新增：波纹扰动控制属性 ---[NoScaleOffset] _NoiseTex ("Noise Texture (波纹噪声图)", 2D) = "grey" {}
        _NoiseTex ("Noise Texture", 2D) = "grey" {}
        _NoiseScale ("Noise Scale (波纹密集度)", Float) = 5.0
        _Distortion ("Distortion (扰动强度)", Range(0, 0.1)) = 0.02
        _SpeedX ("Speed X (水平流速)", Range(-1, 1)) = 0.05
        _SpeedY ("Speed Y (垂直流速)", Range(-1, 1)) = 0.02

        [Header(Sparkle Settings)]
        _SparkleThreshold ("Sparkle Threshold (波峰阈值)", Range(0.4, 1.0)) = 0.8
        _SparkleDensity ("Sparkle Density (闪光点密集度)", Float) = 150.0
        _SparkleSpeed ("Sparkle Twinkle Speed (闪烁速度)", Float) = 5.0
        _SparkleIntensity ("Sparkle Intensity (高光强度)", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
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
            };

            sampler2D _MainTex;
            float _Alpha;
            
            // --- 新增：声明波纹所需的变量 ---
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST; 
            float _NoiseScale;
            float _Distortion;
            float _SpeedX;
            float _SpeedY;

            float _SparkleThreshold;
            float _SparkleDensity;
            float _SparkleSpeed;
            float _SparkleIntensity;
            float GetRandomNoise(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 翻转原图 UV（你的原逻辑）
                float2 staticUV = i.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
                float2 flippedUV = float2(i.uv.x, 1.0 - i.uv.y);
                
                // 2. 计算滚动的波纹 UV
                // 使用 _Time.y (游戏运行秒数) 乘以速度，让 UV 随时间移动
                float2 noiseUV = i.uv * _NoiseScale;
                noiseUV.x += _Time.y * _SpeedX;
                noiseUV.y += _Time.y * _SpeedY;
                
                // 3. 采样噪声贴图
                // tex2D 获取到的颜色值在 0~1 之间。减去 0.5 是为了让偏移量包含正负方向（-0.5 ~ 0.5）
                // 这样波纹会向四周均匀扭曲，而不是只向一个方向平移
                float noiseVal = tex2D(_NoiseTex, noiseUV).r;
                float2 offset = tex2D(_NoiseTex, noiseUV).rr * 2.0 - 1.0;
                
                // 4. 将扰动叠加到主贴图的 UV 上
                flippedUV += offset * _Distortion;

                // 5. 采样主贴图并输出
                fixed4 col = tex2D(_MainTex, flippedUV);
                col.a *= _Alpha;
                
                // 【提取波峰】用 smoothstep 过滤掉低于阈值的部分，只留下最亮的波峰
                // noiseVal 越接近 1，peak 越接近 1
                float peak = smoothstep(_SparkleThreshold, 1.0, noiseVal);

                // 【划分网格】将连续的 UV 放大并向下取整，制造出离散的“马赛克格子”
                // 这样随机出来的点就是正方形的像素块，并且会**紧紧跟着波纹流动**！
                // float2 gridUV = floor(noiseUV * _SparkleDensity);
                
                // 【调用刚刚声明的函数】

                // 2. 用静态 UV 采样光斑，光斑位置就被“钉死”在原地了
                float randVal = tex2D(_NoiseTex, staticUV * (_SparkleDensity * 0.05)).r;
                
                float dotMask = smoothstep(_SparkleThreshold, 1.0, randVal);
                float twinkle = sin(_Time.y * _SparkleSpeed + randVal * 100.0) * 0.5 + 0.5;
                float finalSparkle = peak * dotMask * twinkle * _SparkleIntensity;

                // 6. 强制将水波颜色推向纯白
                // rgb += 就是发光叠加（Additive）。如果是带 HDR 颜色的项目，甚至可以泛发光晕。
                col.rgb += finalSparkle;
                // 确保高光处的像素是不透明的，不会半透明消失
                col.a = saturate(col.a + finalSparkle);

                return col;
            }
            ENDCG
        }
    }
}