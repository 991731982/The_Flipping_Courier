Shader "Hidden/GravityOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UpColor ("Up Color", Color) = (0.3, 0.5, 0.9, 0.05)
        _DownColor ("Down Color", Color) = (0.9, 0.4, 0.3, 0.05)
        _Transition ("Transition", Range(0, 1)) = 0
        _GradientIntensity ("Gradient Intensity", Range(0, 1)) = 0.3
        _GradientWidth ("Gradient Width", Range(0, 2)) = 2.0
        _ArrowOpacity ("Arrow Opacity", Range(0, 1)) = 0.15
        _ArrowScale ("Arrow Scale", Range(0.5, 3)) = 1
        _ArrowSpeed ("Arrow Animation Speed", Range(0.1, 2)) = 0.8
        _EnableBreathing ("Enable Breathing", Float) = 1
        _BreathingIntensity ("Breathing Intensity", Range(0.01, 1)) = 0.05
        _BreathingSpeed ("Breathing Speed", Range(0.1, 2)) = 0.3
    }
    
    SubShader
    {
        Tags { "RenderType"="Overlay" "Queue"="Overlay+100" }
        LOD 100
        
        Pass
        {
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            
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
                float2 screenPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            fixed4 _UpColor;
            fixed4 _DownColor;
            float _Transition;
            float _GradientIntensity;
            float _GradientWidth;
            float _ArrowOpacity;
            float _ArrowScale;
            float _ArrowSpeed;
            float _EnableBreathing;
            float _BreathingIntensity;
            float _BreathingSpeed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = o.uv;
                return o;
            }
            
            // Smooth step function for better gradients
            float smootherstep(float edge0, float edge1, float x)
            {
                x = saturate((x - edge0) / (edge1 - edge0));
                return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
            }
            
            // Arrow shape function
            float drawArrow(float2 uv, float2 pos, float rotation, float scale)
            {
                // Rotate UV
                float2 rotUV = uv - pos;
                float cosR = cos(rotation);
                float sinR = sin(rotation);
                rotUV = float2(
                    rotUV.x * cosR - rotUV.y * sinR,
                    rotUV.x * sinR + rotUV.y * cosR
                ) / scale;
                
                // Arrow body (triangle)
                float arrowBody = step(abs(rotUV.x), 0.015) * step(rotUV.y, 0.08) * step(-0.05, rotUV.y);
                
                // Arrow head
                float arrowHead = step(abs(rotUV.x) + (rotUV.y - 0.08) * 2.0, 0.05) * step(0.08, rotUV.y) * step(rotUV.y, 0.12);
                
                return max(arrowBody, arrowHead);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 screenUV = i.screenPos;
                
                // Create gradient from sides to center
                float gradientMask = 1.0 - smootherstep(0.0, _GradientWidth * 0.5, 
                    min(screenUV.x, 1.0 - screenUV.x));
                gradientMask = pow(gradientMask, 2.0) * _GradientIntensity;
                
                // Interpolate between down and up colors based on transition
                fixed4 overlayColor = lerp(_DownColor, _UpColor, _Transition);
                overlayColor.a *= gradientMask;
                
                // Add optional breathing effect
                if (_EnableBreathing > 0.5)
                {
                    float breathe = sin(_Time.y * _BreathingSpeed) * _BreathingIntensity + (1.0 - _BreathingIntensity);
                    overlayColor.a *= breathe;
                }
                
                // Arrow effects
                if (_ArrowOpacity > 0.001)
                {
                    float arrowAlpha = 0;
                    float time = _Time.y * _ArrowSpeed;
                    
                    // Main arrows positioned based on gravity direction
                    float rotation = _Transition > 0.5 ? 0 : 3.14159; // 0 for up, PI for down
                    
                    // Define arrow positions for the 3-step animation
                    float baseY = _Transition > 0.5 ? 0.75 : 0.25; // Top half or bottom half
                    float direction = _Transition > 0.5 ? 1.0 : -1.0; // Up or down movement
                    
                    // 3-step sequential arrow animation
                    for (int x = 0; x < 3; x++) // 3 columns
                    {
                        float xPos = 0.2 + float(x) * 0.3; // Spread across width
                        
                        for (int step = 0; step < 3; step++) // 3 animation steps
                        {
                            // Stagger the timing for each step
                            float stepTime = time + float(x) * 0.3 + float(step) * 0.5;
                            float cyclePhase = fmod(stepTime, 3.0); // 3-second cycle
                            
                            // Calculate this step's visibility
                            float stepStart = float(step);
                            float stepPeak = float(step) + 0.8;
                            float stepEnd = float(step) + 1.5;
                            
                            float stepAlpha = 0;
                            if (cyclePhase >= stepStart && cyclePhase <= stepEnd)
                            {
                                if (cyclePhase <= stepPeak)
                                {
                                    // Fade in
                                    stepAlpha = smoothstep(stepStart, stepPeak, cyclePhase);
                                }
                                else
                                {
                                    // Fade out
                                    stepAlpha = 1.0 - smoothstep(stepPeak, stepEnd, cyclePhase);
                                }
                            }
                            
                            // Position arrow for this step
                            float stepOffset = float(step) * 0.08 * direction; // Move up/down
                            float2 arrowPos = float2(xPos, baseY + stepOffset);
                            
                            // Only draw if within screen bounds
                            if (arrowPos.y >= 0.1 && arrowPos.y <= 0.9)
                            {
                                float arrowShape = drawArrow(screenUV, arrowPos, rotation, _ArrowScale);
                                arrowAlpha += arrowShape * stepAlpha * 0.4;
                            }
                        }
                    }
                    
                    // Smaller counter-arrows (static, subtle)
                    float counterRotation = _Transition > 0.5 ? 3.14159 : 0;
                    float counterY = _Transition > 0.5 ? 0.35 : 0.65;
                    
                    for (int k = 0; k < 2; k++)
                    {
                        float2 counterPos = float2(0.3 + float(k) * 0.4, counterY);
                        float counterWave = sin(time * 0.8 + float(k) * 1.2) * 0.2 + 0.3;
                        float counterArrow = drawArrow(screenUV, counterPos, counterRotation, _ArrowScale * 0.4);
                        arrowAlpha += counterArrow * counterWave * 0.1;
                    }
                    
                    // Blend arrows with transition state
                    float transitionAlpha = abs(_Transition - 0.5) * 2.0;
                    arrowAlpha *= _ArrowOpacity * (0.6 + transitionAlpha * 0.4);
                    
                    // Apply arrow overlay
                    fixed4 arrowColor = lerp(_DownColor, _UpColor, _Transition);
                    arrowColor.a = arrowAlpha;
                    overlayColor = fixed4(
                        overlayColor.rgb + arrowColor.rgb * arrowColor.a,
                        overlayColor.a + arrowColor.a
                    );
                }
                
                // Smooth transition effect during gravity flip (much gentler)
                float transitionGlow = sin(_Transition * 3.14159) * 0.03;
                overlayColor.a += transitionGlow * gradientMask;
                
                // Final blend
                return fixed4(
                    lerp(col.rgb, overlayColor.rgb, overlayColor.a),
                    col.a
                );
            }
            ENDCG
        }
    }
}