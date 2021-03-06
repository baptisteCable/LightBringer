Shader "Effects/CrackedGround" {
	Properties{
		_MainTex("MainTex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)

		_DepthTex("Depth texture", 2D) = "white" {}
		_TexScale("Depth texture scale", Range(.1, 10)) = 1
		_Height("Height", Range(0.001,.2)) = 1.0
		_DepthColor("DepthColor", Color) = (1,1,1,1)

		_CrackTextureTop("Crack border top texture", 2D) = "white" {}
		_CrackColorTop("Crack border top color", Color) = (1,1,1,1)

		_CrackTextureBot("Crack border bottom texture", 2D) = "white" {}
		_CrackColorBot("Crack border bottom color", Color) = (1,1,1,1)

		_SurfaceTex("Surface", 2D) = "white" {}
		_SurfaceColor("Surface color", Color) = (1,1,1,1)

		_StepDistance("Step distance", float) = .001
		_Steps("Max number of steps", int) = 300
	}
		SubShader{
			Tags {
				"IgnoreProjector" = "True"
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}
			Pass {
				Name "FORWARD"
				Tags {
					"LightMode" = "ForwardBase"
				}
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "Assets/Shaders/Functions/geometryFunctions.cginc"
				#pragma target 3.0

				uniform sampler2D _MainTex;
				float4 _Color;
				uniform float4 _MainTex_ST;

				//Input
				sampler2D _DepthTex;
				float4 _DepthColor;
				float _TexScale;
				sampler2D _CrackTextureTop;
				float4 _CrackColorTop;
				sampler2D _CrackTextureBot;
				float4 _CrackColorBot;
				sampler2D _SurfaceTex;
				float4 _SurfaceColor;
				float _Height;
				float _StepDistance;
				int _Steps;

				struct VertexInput {
					float4 vertex : POSITION;
					float2 texcoord0 : TEXCOORD0;
					float4 vertexColor : COLOR;
					float4 tangent : TANGENT;
					float3 normal : NORMAL;
				};

				struct VertexOutput {
					//float2 uv_NoiseTex;

					float4 pos : SV_POSITION;
					float2 uv0 : TEXCOORD0;
					float4 vertexColor : COLOR;
					float3 tangentViewDir : TAN_VIEW_DIR;
				};

				void vert(VertexInput v, out VertexOutput o) {
					// Base

					UNITY_INITIALIZE_OUTPUT(VertexOutput, o);

					o.uv0 = v.texcoord0;
					o.vertexColor = v.vertexColor;
					o.pos = UnityObjectToClipPos(v.vertex);

					//Transform the view direction from world space to tangent space			
					float3 worldVertexPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					float3 worldViewDir = worldVertexPos - _WorldSpaceCameraPos;

					//To convert from world space to tangent space we need the following
					float3 worldNormal = UnityObjectToWorldNormal(v.normal);
					float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
					float3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w * unity_WorldTransformParams.w;

					//Use dot products instead of building the matrix
					o.tangentViewDir = float3(
						dot(worldViewDir, worldTangent),
						dot(worldViewDir, worldNormal),
						dot(worldViewDir, worldBitangent)
						);
				}


				//Get the height from a uv position
				float getHeight(float2 texturePos)
				{
					// Height from texture
					float4 colorNoise = tex2Dlod(_DepthTex, float4(texturePos * _TexScale, 0, 0));

					//Calculate the height at this uv coordinate
					//-1 because the ray is going down so the ray's y-coordinate will be negative
					float height = -colorNoise.r * _Height;

					return height;
				}


				//Combine stone and grass depending on grayscale color
				float4 getBlendTexture(float2 texturePos, float height)
				{
					//Height is negative so convert it to positive, also invert it so mountains are high and not the grass
					//Divide with _Height because this height is actual height and we need it in 0 -> 1 range
					float relativeDepth = abs(height) / _Height;

					//Combine grass and stone depending on height
					float4 mixedColor;

					//Depth
					if (relativeDepth > 0.8)
					{
						mixedColor = float4(_DepthColor.rgb, _DepthColor.a);
					}
					else if (relativeDepth < .01)
					{
						float4 surfaceColor = tex2Dlod(_SurfaceTex, float4(texturePos, 0, 0));
						mixedColor = surfaceColor * _SurfaceColor;
					}
					else if (relativeDepth > 0.1)
					{
						float4 botColor = tex2Dlod(_CrackTextureBot, float4(texturePos, 0, 0));
						float4 topColor = tex2Dlod(_CrackTextureTop, float4(texturePos, 0, 0));
						mixedColor = lerp(topColor * _CrackColorTop, botColor * _CrackColorBot, (relativeDepth - .1) / .8);
						mixedColor.rgb = mixedColor.rgb + _DepthColor.rgb * relativeDepth;
					}
					else
					{
						float4 topColor = tex2Dlod(_CrackTextureTop, float4(texturePos, 0, 0));
						float4 surfaceColor = tex2Dlod(_SurfaceTex, float4(texturePos, 0, 0));
						mixedColor = lerp(surfaceColor * _SurfaceColor, topColor * _CrackColorTop, relativeDepth / .1);
					}

					return mixedColor;
				}


				//Get the texture position by interpolation between the position where we hit terrain and the position before
				float2 getWeightedTexPos(float3 rayPos, float3 rayDir, float stepDistance, float2 realTexPos)
				{
					//Move one step back to the position before we hit terrain
					float3 oldPos = rayPos - stepDistance * rayDir;

					float oldHeight = getHeight(oldPos.xz);

					//Always positive
					float oldDistToTerrain = abs(oldHeight - oldPos.y);

					float currentHeight = getHeight(rayPos.xz);

					//Always negative
					float currentDistToTerrain = rayPos.y - currentHeight;

					float weight = currentDistToTerrain / (currentDistToTerrain - oldDistToTerrain);

					//Calculate a weighted texture coordinate
					//If height is -2 and oldHeight is 2, then weightedTex is 0.5, which is good because we should use 
					//the exact middle between the coordinates
					float2 weightedTexPos = oldPos.xz * weight + rayPos.xz * (1 - weight);

					return weightedTexPos;
				}

				float4 frag(VertexOutput i) : COLOR {

					//Where is the ray starting? y is up and we always start at the surface
					float3 realTexPos = float3(i.uv0.x, 0, i.uv0.y);
					float3 rayPos = realTexPos;

					//What's the direction of the ray?
					float3 rayDir = normalize(i.tangentViewDir);

					//Find where the ray is intersecting with the terrain with a raymarch algorithm


					//The default color used if the ray doesnt hit anything
					float4 finalColor = 0;

					for (int k = 0; k < _Steps; k++)
					{
						//Get the current height at this uv coordinate
						float height = getHeight(rayPos.xz);

						//If the ray is below the surface
						if (rayPos.y < height)
						{
							//Get the texture position by interpolation between the position where we hit terrain and the position before
							float2 weightedTex = getWeightedTexPos(rayPos, rayDir, _StepDistance, realTexPos);

							float height = getHeight(weightedTex);

							finalColor = getBlendTexture(weightedTex, height);

							//We have hit the terrain so we dont need to loop anymore	
							break;
						}

						//Move along the ray
						rayPos += _StepDistance * rayDir;
					}

					float4 _MainTex_var = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
					finalColor.a *= _MainTex_var.a;

					return finalColor * _Color;
				}
				ENDCG
			}
		}
			CustomEditor "ShaderForgeMaterialInspector"
}
