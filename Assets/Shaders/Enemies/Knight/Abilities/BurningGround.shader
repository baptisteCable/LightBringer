// Shader created with Shader Forge v1.41 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Enhanced by Antoine Guillon / Arkham Development - http://www.arkham-development.com/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.41;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:32716,y:32678,varname:node_4795,prsc:2|emission-2393-OUT,alpha-798-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:32224,y:33171,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:71f808d3b7567fa4a822a94fb7d3a43b,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2393,x:32495,y:32793,varname:node_2393,prsc:2|A-9041-RGB,B-2053-RGB,C-797-RGB,D-9248-OUT,E-6074-RGB;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32224,y:32772,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Color,id:797,x:32224,y:32928,ptovrint:True,ptlb:Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Vector1,id:9248,x:32224,y:33088,varname:node_9248,prsc:2,v1:2;n:type:ShaderForge.SFN_Multiply,id:798,x:32495,y:32923,varname:node_798,prsc:2|A-2053-A,B-797-A,C-6074-A;n:type:ShaderForge.SFN_UVTile,id:1249,x:32035,y:32632,varname:node_1249,prsc:2|UVIN-5609-OUT,WDT-5581-OUT,HGT-7084-OUT,TILE-1657-OUT;n:type:ShaderForge.SFN_Vector1,id:5581,x:31771,y:32681,varname:node_5581,prsc:2,v1:8;n:type:ShaderForge.SFN_Vector1,id:7084,x:31771,y:32743,varname:node_7084,prsc:2,v1:4;n:type:ShaderForge.SFN_Time,id:5221,x:31197,y:32739,varname:node_5221,prsc:2;n:type:ShaderForge.SFN_Floor,id:5387,x:31609,y:32821,varname:node_5387,prsc:2|IN-7958-OUT;n:type:ShaderForge.SFN_Fmod,id:1657,x:31801,y:32821,varname:node_1657,prsc:2|A-5387-OUT,B-3315-OUT;n:type:ShaderForge.SFN_Vector1,id:3315,x:31609,y:33002,varname:node_3315,prsc:2,v1:32;n:type:ShaderForge.SFN_Multiply,id:7958,x:31414,y:32821,varname:node_7958,prsc:2|A-5221-T,B-1108-OUT;n:type:ShaderForge.SFN_Vector1,id:1108,x:31197,y:32885,varname:node_1108,prsc:2,v1:30;n:type:ShaderForge.SFN_Tex2d,id:9041,x:32224,y:32584,ptovrint:False,ptlb:NoiseSequence,ptin:_NoiseSequence,varname:node_9041,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:ca7668105b1cd394198729cd7177e20f,ntxv:0,isnm:False|UVIN-1249-UVOUT;n:type:ShaderForge.SFN_FragmentPosition,id:9143,x:31068,y:32288,varname:node_9143,prsc:2;n:type:ShaderForge.SFN_Append,id:4255,x:31276,y:32309,varname:node_4255,prsc:2|A-9143-X,B-9143-Z;n:type:ShaderForge.SFN_Floor,id:4147,x:31479,y:32357,varname:node_4147,prsc:2|IN-4255-OUT;n:type:ShaderForge.SFN_Subtract,id:5609,x:31660,y:32309,varname:node_5609,prsc:2|A-4255-OUT,B-4147-OUT;proporder:797-6074-9041;pass:END;sub:END;*/

Shader "Shader Forge/BurningGround" {
    Properties {
        _TintColor ("Color", Color) = (0.5,0.5,0.5,1)
        _MainTex ("MainTex", 2D) = "black" {}
        _NoiseSequence ("NoiseSequence", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #ifndef UNITY_PASS_FORWARDBASE
            #define UNITY_PASS_FORWARDBASE
            #endif //UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform sampler2D _NoiseSequence; uniform float4 _NoiseSequence_ST;
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
////// Lighting:
////// Emissive:
                float node_5581 = 8.0;
                float4 node_5221 = _Time;
                float node_1657 = fmod(floor((node_5221.g*30.0)),32.0);
                float2 node_1249_tc_rcp = float2(1.0,1.0)/float2( node_5581, 4.0 );
                float node_1249_ty = floor(node_1657 * node_1249_tc_rcp.x);
                float node_1249_tx = node_1657 - node_5581 * node_1249_ty;
                float2 node_4255 = float2(i.posWorld.r,i.posWorld.b);
                float2 node_1249 = ((node_4255-floor(node_4255)) + float2(node_1249_tx, node_1249_ty)) * node_1249_tc_rcp;
                float4 _NoiseSequence_var = tex2D(_NoiseSequence,TRANSFORM_TEX(node_1249, _NoiseSequence));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float3 emissive = (_NoiseSequence_var.rgb*i.vertexColor.rgb*_TintColor.rgb*2.0*_MainTex_var.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(i.vertexColor.a*_TintColor.a*_MainTex_var.a));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
