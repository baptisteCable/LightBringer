Shader "Scenery/WindShaderSeeThrough"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Radius("Radius", Float) = 10
		_Height("Height", Float) = 5
		_WindDir("Wind direction", Vector) = (1, 0, 0, 0)
		_WindStrength("Wind strength", Float) = 1
		_WindFrequency("Wind frequency", Float) = 1
		_Emission("Emission", Color) = (0,0,0,0)
		_EmissionIntensity("EmissionIntensity", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 200

		Pass
		{
			ZWrite On
			ColorMask 0

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Assets/Shaders/Functions/windFunctions.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			float _Radius;
			float _XCenter[20];
			float _ZCenter[20];
			float _Amplitude[20];
			int _ExplosionCount;
			// float _Amplitude1;
			// float4 _Center1;
			float _Height;
			float4 _WindDir;
			float _WindStrength;
			float _WindFrequency;

			v2f vert(appdata v) {
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				worldPos.xyz += windMovement(worldPos, _WindDir, _WindFrequency, _WindStrength, _Height);
				for (int i = 0; i < _ExplosionCount; i++) {
					worldPos.xyz += explosionMovement(worldPos, float2(_XCenter[i], _ZCenter[i]), _Amplitude[i], _Radius, _Height);
				}
				v2f o;
				o.vertex = UnityObjectToClipPos(mul(unity_WorldToObject, worldPos));
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
					return 0;
			}
			ENDCG
		}

        CGPROGRAM
		#pragma vertex vert

        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow alpha
	
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		#include "Assets/Shaders/Functions/windFunctions.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

		half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		half3 _Emission;
		half _EmissionIntensity;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		float _Radius;
		float _XCenter[20];
		float _ZCenter[20];
		float _Amplitude[20];
		int _ExplosionCount;
		// float _Amplitude1;
		// float4 _Center1;
		float _Height;
		float4 _WindDir;
		float _WindStrength;
		float _WindFrequency;

		void vert(inout appdata_full v) {
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
			worldPos.xyz += windMovement(worldPos, _WindDir, _WindFrequency, _WindStrength, _Height);
			for (int i = 0; i < _ExplosionCount; i++) {
				worldPos.xyz += explosionMovement(worldPos, float2(_XCenter[i], _ZCenter[i]), _Amplitude[i], _Radius, _Height);
			}
			v.vertex = mul(unity_WorldToObject, worldPos);
		}
		
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic * _EmissionIntensity;
            o.Smoothness = _Glossiness * _EmissionIntensity;
			o.Emission = _Emission * _EmissionIntensity;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
