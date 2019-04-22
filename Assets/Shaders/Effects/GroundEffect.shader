Shader "Effects/GroundEffect" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Color", Color) = (0.5,0.5,0.5,1)
        _Voronoi ("Voronoi", 2D) = "white" {}
        _VoronoiScale ("Voronoi Scale", Float ) = 10
		_NoiseTexture("NoiseTexture", 2D) = "white" {}
        _Speed ("Speed", Float ) = 0.04
        _Swap ("Swap", Float ) = 0.02
        _Density ("Density", Float ) = 0.01
		_IntensityScale("IntensityScale", Float) = 50
		_IntensityCoeff("IntensityCoeff", Range(1, 10)) = 2
		_IntensityWidth("IntensityWidth", Float) = 0.1
		_IntensityBlurWidth("IntensityBlurWidth", Float) = 0.1
		_IntensitySpeed("IntensitySpeed", Float) = 1
		_IntensityLatency("IntensityLatency", Range(1, 100)) = 2
		_IntensityTimeMod("IntensityTimeMod", Float) = 0.001
        _LightScale ("LightScale", Float ) = 50
        _LightColor ("LightColor", Color) = (1,1,1,1)
		_LightWidth("LightWidth", Float) = 0.1
		_LightBlurWidth("LightBlurWidth", Float) = 0.1
        _LightSpeed ("LightSpeed", Float ) = 1
        _LightLatency ("LightLatency", Range(1, 100)) = 2
        _LightTimeMod ("LightTimeMod", Float ) = 0.001
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #ifndef UNITY_PASS_FORWARDBASE
            #define UNITY_PASS_FORWARDBASE
            #endif //UNITY_PASS_FORWARDBASE
			#include "UnityCG.cginc"
			#include "Assets/Shaders/Functions/geometryFunctions.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform sampler2D _Voronoi; uniform float4 _Voronoi_ST;
            uniform float _VoronoiScale;
            uniform float _Speed;
            uniform float _Swap;
            uniform float _Density;
            uniform sampler2D _NoiseTexture; uniform float4 _NoiseTexture_ST;
			uniform float _IntensityScale;
			uniform float _IntensityCoeff;
			uniform float _IntensityWidth;
			uniform float _IntensityBlurWidth;
			uniform float _IntensitySpeed;
			uniform float _IntensityLatency;
			uniform float _IntensityTimeMod;
			uniform float _LightScale;
			uniform float4 _LightColor;
			uniform float _LightWidth;
			uniform float _LightBlurWidth;
			uniform float _LightSpeed;
			uniform float _LightLatency;
			uniform float _LightTimeMod;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
				float4 _MainTex_var = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));

				// light world position
				float2 timeModPos = (i.uv0 * _IntensityTimeMod);

				// Time modifier depending on noise value. in [0, 1]
				float timeMod = tex2D(_NoiseTexture,TRANSFORM_TEX(timeModPos, _NoiseTexture)).r;

				// Bounds in wich the voronoi is accentuented
				float lightLowerBound = fmod(((timeMod * _IntensityLatency) + (_Time.g * _IntensitySpeed)),_IntensityLatency);
				float lightUpperBound = _IntensityWidth + lightLowerBound;

				// Voronoi intensity if between bounds
				float2 lightPos = translate(i.uv0, _Time.g / 10) * _IntensityScale;
				float noiseLight = tex2D(_NoiseTexture, TRANSFORM_TEX(lightPos, _NoiseTexture)).r;
				float d = abs((lightLowerBound + lightUpperBound) / 2 - noiseLight);
				float l = (lightUpperBound - lightLowerBound) / 2;
				float attenuation = 1 - step(l, d) * (d - l) / _IntensityBlurWidth;
				float voronoiIntensity = step(lightLowerBound - _IntensityBlurWidth, noiseLight) * step(noiseLight, lightUpperBound + _IntensityBlurWidth) * attenuation;
				voronoiIntensity = (1 - voronoiIntensity) / _IntensityCoeff + voronoiIntensity;

				// light world position
				timeModPos = (i.uv0 * _LightTimeMod);

				// Time modifier depending on noise value. in [0, 1]
				timeMod = tex2D(_NoiseTexture, TRANSFORM_TEX(timeModPos, _NoiseTexture)).r;

				// Bounds in wich the light is displayed
				lightLowerBound = fmod(((timeMod * _LightLatency) + (_Time.g * _LightSpeed)), _LightLatency);
				lightUpperBound = _LightWidth + lightLowerBound;

				// Light if between bounds
				lightPos = translate(i.uv0, _Time.g / 10) * _LightScale;
				noiseLight = tex2D(_NoiseTexture, TRANSFORM_TEX(lightPos, _NoiseTexture)).r;
				d = abs((lightLowerBound + lightUpperBound) / 2 - noiseLight);
				l = (lightUpperBound - lightLowerBound) / 2;
				attenuation = 1 - step(l, d) * (d - l) / _LightBlurWidth;
				float lightIntensity = step(lightLowerBound - _LightBlurWidth, noiseLight) * step(noiseLight, lightUpperBound + _LightBlurWidth) * attenuation;
				float3 light = lightIntensity * _LightColor.rgb * _LightColor.a * _MainTex_var.a * i.vertexColor.a;


				// Position to read in line or column in noise
				float posToReadInNoise = fmod((_Time.g * _Speed),1.0);

				// World pos with noise density on Voronoi
				float xScaledWorldPos = i.uv0.r * _Density;
				float yScaledWorldPos = i.uv0.g * _Density;

				// Noise on each coordinate
				float2 xNoiseCoord = float2(posToReadInNoise,(xScaledWorldPos - floor(xScaledWorldPos)));
				float2 yNoiseCoord = float2((yScaledWorldPos - floor(yScaledWorldPos)), posToReadInNoise);
				float xNoise = tex2D(_NoiseTexture,TRANSFORM_TEX(xNoiseCoord, _NoiseTexture)).r;
				float yNoise = tex2D(_NoiseTexture,TRANSFORM_TEX(yNoiseCoord, _NoiseTexture)).r;

				// Noised world coordinate for Voronoi
				float2 vonoroiWorldCoord = float2(((i.uv0.y / _VoronoiScale) + (xNoise * _Swap)), ((i.uv0.x / _VoronoiScale) + (_Swap * yNoise)));
				float4 _Voronoi_var = tex2D(_Voronoi,TRANSFORM_TEX(vonoroiWorldCoord, _Voronoi));


				float3 finalColor = (light + ((_MainTex_var.rgb * i.vertexColor.rgb * i.vertexColor.a * _TintColor.rgb * _TintColor.a)
					* (_MainTex_var.a * 2.0 * _Voronoi_var.a * voronoiIntensity)));
				
				fixed4 finalRGBA = fixed4(finalColor,1);
				
				UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
				return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
