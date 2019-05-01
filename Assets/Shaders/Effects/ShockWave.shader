Shader "Effect/ShockWave" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
		_Strength("Strength", float) = 1.0
        _TintColor ("Color", Color) = (0.5,0.5,0.5,1)
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }



		// Grab the screen behind the object into _BackgroundTexture
		GrabPass
		{
			"_BackgroundTexture"
		}

        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }

			ZTest Always
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #ifndef UNITY_PASS_FORWARDBASE
            #define UNITY_PASS_FORWARDBASE
            #endif //UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma target 3.0

            uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
			sampler2D _BackgroundTexture;
			float _Strength;

            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct VertexOutput {
                float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float4 grabPos : TEXCOORD1;
                float4 vertexColor : COLOR;
            };

            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );

				o.grabPos = ComputeGrabScreenPos(o.pos);

                return o;
            }

			float from255ToDouble(float x)
			{
				return (x - .5) * 2.0;
			}

			float4 frag(VertexOutput i) : COLOR{
				float4 dist = tex2Dlod(_MainTex, float4(i.uv0, 0, 0));

				i.grabPos.x += from255ToDouble(dist.r) * _Strength * dist.a * _TintColor.a * i.vertexColor.a;
				i.grabPos.y += from255ToDouble(dist.g) * _Strength * dist.a * _TintColor.a * i.vertexColor.a;

				float4 finalColor = tex2Dproj(_BackgroundTexture, i.grabPos);
				finalColor.rgb += _TintColor.rgb;
				
				return finalColor;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
