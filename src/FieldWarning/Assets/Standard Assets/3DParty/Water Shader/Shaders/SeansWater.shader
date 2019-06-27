// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:3,spmd:0,trmd:1,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:True,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.6509434,fgcg:0.6509434,fgcb:0.6509434,fgca:1,fgde:0.05,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9399,x:32719,y:32712,varname:node_9399,prsc:2|diff-9617-RGB,spec-9689-RGB,gloss-2914-OUT,normal-7206-OUT,alpha-2950-OUT,refract-1337-OUT;n:type:ShaderForge.SFN_Slider,id:2914,x:32111,y:32833,ptovrint:False,ptlb:Gloss Amount,ptin:_GlossAmount,varname:node_2914,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Color,id:9689,x:32111,y:32658,ptovrint:False,ptlb:Specular Color,ptin:_SpecularColor,varname:_node_2314_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Tex2dAsset,id:9344,x:30204,y:32609,ptovrint:False,ptlb:Wave Normals,ptin:_WaveNormals,varname:node_9344,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:2dd3788f8589b40bf82a92d76ffc5091,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Tex2d,id:9564,x:30492,y:32600,varname:node_9564,prsc:2,tex:2dd3788f8589b40bf82a92d76ffc5091,ntxv:0,isnm:False|UVIN-5163-OUT,TEX-9344-TEX;n:type:ShaderForge.SFN_Tex2d,id:7724,x:30492,y:32724,varname:node_7724,prsc:2,tex:2dd3788f8589b40bf82a92d76ffc5091,ntxv:0,isnm:False|UVIN-1597-UVOUT,TEX-9344-TEX;n:type:ShaderForge.SFN_Blend,id:649,x:30694,y:32665,varname:node_649,prsc:2,blmd:8,clmp:False|SRC-9564-RGB,DST-7724-RGB;n:type:ShaderForge.SFN_Panner,id:8217,x:30047,y:32568,varname:node_8217,prsc:2,spu:1,spv:0|UVIN-637-UVOUT,DIST-6665-OUT;n:type:ShaderForge.SFN_Panner,id:1597,x:30086,y:32819,varname:node_1597,prsc:2,spu:0,spv:1|UVIN-55-UVOUT,DIST-6665-OUT;n:type:ShaderForge.SFN_TexCoord,id:637,x:29818,y:32584,varname:node_637,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_TexCoord,id:55,x:29818,y:32831,varname:node_55,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:5188,x:29519,y:32716,varname:node_5188,prsc:2;n:type:ShaderForge.SFN_Slider,id:4937,x:29494,y:32601,ptovrint:False,ptlb:Wave Scroll Speed,ptin:_WaveScrollSpeed,varname:node_4937,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:0.1;n:type:ShaderForge.SFN_Multiply,id:6665,x:29701,y:32716,varname:node_6665,prsc:2|A-4937-OUT,B-5188-TSL;n:type:ShaderForge.SFN_Slider,id:6820,x:30849,y:32840,ptovrint:False,ptlb:Wave Intensity,ptin:_WaveIntensity,varname:node_6820,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1.5,cur:1,max:1.5;n:type:ShaderForge.SFN_Lerp,id:6711,x:31249,y:32743,varname:node_6711,prsc:2|A-649-OUT,B-2049-OUT,T-6820-OUT;n:type:ShaderForge.SFN_Vector3,id:2049,x:30851,y:32737,varname:node_2049,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Multiply,id:5163,x:30273,y:32451,varname:node_5163,prsc:2|A-8217-UVOUT,B-7056-OUT;n:type:ShaderForge.SFN_Vector1,id:7056,x:30047,y:32714,varname:node_7056,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Tex2dAsset,id:6660,x:30213,y:32228,ptovrint:False,ptlb:Detail Normals,ptin:_DetailNormals,varname:_WaveNormals_copy,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:2dd3788f8589b40bf82a92d76ffc5091,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Blend,id:2059,x:30663,y:32201,varname:node_2059,prsc:2,blmd:8,clmp:False|SRC-2498-RGB,DST-6823-RGB;n:type:ShaderForge.SFN_Panner,id:6697,x:30010,y:32107,varname:node_6697,prsc:2,spu:1,spv:0|UVIN-4528-UVOUT,DIST-9285-OUT;n:type:ShaderForge.SFN_Panner,id:6125,x:30049,y:32358,varname:node_6125,prsc:2,spu:0,spv:1|UVIN-5877-UVOUT,DIST-9285-OUT;n:type:ShaderForge.SFN_TexCoord,id:4528,x:29701,y:31920,varname:node_4528,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_TexCoord,id:5877,x:29781,y:32370,varname:node_5877,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Time,id:1568,x:29356,y:32207,varname:node_1568,prsc:2;n:type:ShaderForge.SFN_Slider,id:3757,x:29456,y:32140,ptovrint:False,ptlb:Detail Scroll Speed,ptin:_DetailScrollSpeed,varname:_NormalScrollSpeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:0.1;n:type:ShaderForge.SFN_Multiply,id:9285,x:29663,y:32254,varname:node_9285,prsc:2|A-3757-OUT,B-1568-TSL;n:type:ShaderForge.SFN_Multiply,id:8611,x:30213,y:32074,varname:node_8611,prsc:2|A-6697-UVOUT,B-2304-OUT;n:type:ShaderForge.SFN_Vector1,id:2304,x:30010,y:32253,varname:node_2304,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Tex2d,id:2498,x:30454,y:32117,varname:node_2498,prsc:2,tex:2dd3788f8589b40bf82a92d76ffc5091,ntxv:0,isnm:False|UVIN-8611-OUT,TEX-6660-TEX;n:type:ShaderForge.SFN_Tex2d,id:6823,x:30438,y:32287,varname:node_6823,prsc:2,tex:2dd3788f8589b40bf82a92d76ffc5091,ntxv:0,isnm:False|UVIN-6125-UVOUT,TEX-6660-TEX;n:type:ShaderForge.SFN_Vector3,id:7666,x:30851,y:32428,varname:node_7666,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Lerp,id:4953,x:31249,y:32487,varname:node_4953,prsc:2|A-2059-OUT,B-7666-OUT,T-8940-OUT;n:type:ShaderForge.SFN_Slider,id:8940,x:30831,y:32526,ptovrint:False,ptlb:Detail Intensity,ptin:_DetailIntensity,varname:_WaveIntensity_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:1.5;n:type:ShaderForge.SFN_Blend,id:9738,x:31507,y:32622,varname:node_9738,prsc:2,blmd:8,clmp:False|SRC-4953-OUT,DST-6711-OUT;n:type:ShaderForge.SFN_Lerp,id:8487,x:31130,y:31667,varname:node_8487,prsc:2|A-6451-OUT,B-3587-OUT,T-5296-OUT;n:type:ShaderForge.SFN_Vector3,id:3587,x:30732,y:31661,varname:node_3587,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Blend,id:6451,x:30575,y:31589,varname:node_6451,prsc:2,blmd:8,clmp:False|SRC-4502-RGB,DST-7637-RGB;n:type:ShaderForge.SFN_Multiply,id:3213,x:30154,y:31375,varname:node_3213,prsc:2|A-3361-UVOUT,B-8750-OUT;n:type:ShaderForge.SFN_Panner,id:3361,x:29909,y:31484,varname:node_3361,prsc:2,spu:1,spv:0|UVIN-5315-UVOUT,DIST-8964-OUT;n:type:ShaderForge.SFN_Panner,id:6353,x:29968,y:31740,varname:node_6353,prsc:2,spu:0,spv:1|UVIN-937-UVOUT,DIST-8964-OUT;n:type:ShaderForge.SFN_TexCoord,id:5315,x:29645,y:31367,varname:node_5315,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_TexCoord,id:937,x:29700,y:31752,varname:node_937,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:8964,x:29583,y:31637,varname:node_8964,prsc:2|A-953-OUT,B-7107-TSL;n:type:ShaderForge.SFN_Time,id:7107,x:29401,y:31637,varname:node_7107,prsc:2;n:type:ShaderForge.SFN_Slider,id:953,x:29376,y:31522,ptovrint:False,ptlb:Minute Detail Scroll Speed,ptin:_MinuteDetailScrollSpeed,varname:_WaveScrollSpeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:0.1;n:type:ShaderForge.SFN_Vector1,id:8750,x:29909,y:31637,varname:node_8750,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Blend,id:7206,x:31765,y:32327,varname:node_7206,prsc:2,blmd:8,clmp:False|SRC-8487-OUT,DST-9738-OUT;n:type:ShaderForge.SFN_Tex2d,id:4502,x:30375,y:31478,varname:node_4502,prsc:2,tex:fb6566c21f717904f83743a5a76dd0b0,ntxv:0,isnm:False|UVIN-3213-OUT,TEX-4606-TEX;n:type:ShaderForge.SFN_Tex2d,id:7637,x:30371,y:31665,varname:node_7637,prsc:2,tex:fb6566c21f717904f83743a5a76dd0b0,ntxv:0,isnm:False|UVIN-6353-UVOUT,TEX-4606-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:4606,x:30111,y:31586,ptovrint:False,ptlb:Minute Detail Normals,ptin:_MinuteDetailNormals,varname:node_4606,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:fb6566c21f717904f83743a5a76dd0b0,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Slider,id:5296,x:30715,y:31848,ptovrint:False,ptlb:Minute Detail Intensity,ptin:_MinuteDetailIntensity,varname:node_5296,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:1.5;n:type:ShaderForge.SFN_Vector1,id:3115,x:30138,y:32381,varname:node_3115,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Vector1,id:9093,x:30202,y:32445,varname:node_9093,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Vector1,id:3628,x:30266,y:32509,varname:node_3628,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Vector1,id:938,x:30330,y:32573,varname:node_938,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Vector1,id:7503,x:30394,y:32637,varname:node_7503,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Vector1,id:7903,x:30458,y:32701,varname:node_7903,prsc:2,v1:1.2;n:type:ShaderForge.SFN_Color,id:9617,x:32272,y:32491,ptovrint:False,ptlb:Diffuse Color,ptin:_DiffuseColor,varname:node_9617,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Slider,id:6777,x:31784,y:32931,ptovrint:False,ptlb:Depth,ptin:_Depth,varname:node_6777,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:20;n:type:ShaderForge.SFN_DepthBlend,id:2950,x:32189,y:32905,varname:node_2950,prsc:2|DIST-6777-OUT;n:type:ShaderForge.SFN_Slider,id:1500,x:31022,y:33462,ptovrint:False,ptlb:Refraction Speed,ptin:_RefractionSpeed,varname:node_7205,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Time,id:6560,x:30910,y:33298,varname:node_6560,prsc:2;n:type:ShaderForge.SFN_Multiply,id:3929,x:31190,y:33221,varname:node_3929,prsc:2|A-6560-TSL,B-1500-OUT;n:type:ShaderForge.SFN_TexCoord,id:1060,x:31091,y:33028,varname:node_1060,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Panner,id:3248,x:31450,y:33150,varname:node_3248,prsc:2,spu:0.1,spv:0.1|UVIN-1060-UVOUT,DIST-3929-OUT;n:type:ShaderForge.SFN_Tex2d,id:5454,x:31625,y:33132,ptovrint:False,ptlb:Refraction Normals,ptin:_RefractionNormals,varname:node_4851,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True|UVIN-3248-UVOUT;n:type:ShaderForge.SFN_ComponentMask,id:6368,x:31841,y:33157,varname:node_6368,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-5454-RGB;n:type:ShaderForge.SFN_Multiply,id:1337,x:32239,y:33276,varname:node_1337,prsc:2|A-6368-OUT,B-3699-OUT;n:type:ShaderForge.SFN_Slider,id:3699,x:31587,y:33330,ptovrint:False,ptlb:Refraction Intensity,ptin:_RefractionIntensity,varname:node_9725,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;proporder:9617-9689-2914-9344-6820-4937-6660-8940-3757-4606-5296-953-6777-5454-3699-1500;pass:END;sub:END;*/

Shader "Custom/SeansWater" {
    Properties {
        _DiffuseColor ("Diffuse Color", Color) = (0.5,0.5,0.5,1)
        _SpecularColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
        _GlossAmount ("Gloss Amount", Range(0, 1)) = 1
        _WaveNormals ("Wave Normals", 2D) = "bump" {}
        _WaveIntensity ("Wave Intensity", Range(-1.5, 1.5)) = 1
        _WaveScrollSpeed ("Wave Scroll Speed", Range(0, 0.1)) = 0
        _DetailNormals ("Detail Normals", 2D) = "bump" {}
        _DetailIntensity ("Detail Intensity", Range(1, 1.5)) = 1
        _DetailScrollSpeed ("Detail Scroll Speed", Range(0, 0.1)) = 0
        _MinuteDetailNormals ("Minute Detail Normals", 2D) = "bump" {}
        _MinuteDetailIntensity ("Minute Detail Intensity", Range(1, 1.5)) = 1
        _MinuteDetailScrollSpeed ("Minute Detail Scroll Speed", Range(0, 0.1)) = 0
        _Depth ("Depth", Range(0, 20)) = 1
        _RefractionNormals ("Refraction Normals", 2D) = "bump" {}
        _RefractionIntensity ("Refraction Intensity", Range(0, 1)) = 0
        _RefractionSpeed ("Refraction Speed", Range(0, 2)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform sampler2D _CameraDepthTexture;
            uniform float _GlossAmount;
            uniform float4 _SpecularColor;
            uniform sampler2D _WaveNormals; uniform float4 _WaveNormals_ST;
            uniform float _WaveScrollSpeed;
            uniform float _WaveIntensity;
            uniform sampler2D _DetailNormals; uniform float4 _DetailNormals_ST;
            uniform float _DetailScrollSpeed;
            uniform float _DetailIntensity;
            uniform float _MinuteDetailScrollSpeed;
            uniform sampler2D _MinuteDetailNormals; uniform float4 _MinuteDetailNormals_ST;
            uniform float _MinuteDetailIntensity;
            uniform float4 _DiffuseColor;
            uniform float _Depth;
            uniform float _RefractionSpeed;
            uniform sampler2D _RefractionNormals; uniform float4 _RefractionNormals_ST;
            uniform float _RefractionIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 projPos : TEXCOORD5;
                UNITY_FOG_COORDS(6)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 node_7107 = _Time;
                float node_8964 = (_MinuteDetailScrollSpeed*node_7107.r);
                float2 node_3213 = ((i.uv0+node_8964*float2(1,0))*1.2);
                float3 node_4502 = UnpackNormal(tex2D(_MinuteDetailNormals,TRANSFORM_TEX(node_3213, _MinuteDetailNormals)));
                float2 node_6353 = (i.uv0+node_8964*float2(0,1));
                float3 node_7637 = UnpackNormal(tex2D(_MinuteDetailNormals,TRANSFORM_TEX(node_6353, _MinuteDetailNormals)));
                float4 node_1568 = _Time;
                float node_9285 = (_DetailScrollSpeed*node_1568.r);
                float2 node_8611 = ((i.uv0+node_9285*float2(1,0))*1.2);
                float3 node_2498 = UnpackNormal(tex2D(_DetailNormals,TRANSFORM_TEX(node_8611, _DetailNormals)));
                float2 node_6125 = (i.uv0+node_9285*float2(0,1));
                float3 node_6823 = UnpackNormal(tex2D(_DetailNormals,TRANSFORM_TEX(node_6125, _DetailNormals)));
                float4 node_5188 = _Time;
                float node_6665 = (_WaveScrollSpeed*node_5188.r);
                float2 node_5163 = ((i.uv0+node_6665*float2(1,0))*1.2);
                float3 node_9564 = UnpackNormal(tex2D(_WaveNormals,TRANSFORM_TEX(node_5163, _WaveNormals)));
                float2 node_1597 = (i.uv0+node_6665*float2(0,1));
                float3 node_7724 = UnpackNormal(tex2D(_WaveNormals,TRANSFORM_TEX(node_1597, _WaveNormals)));
                float3 normalLocal = (lerp((node_4502.rgb+node_7637.rgb),float3(0,0,1),_MinuteDetailIntensity)+(lerp((node_2498.rgb+node_6823.rgb),float3(0,0,1),_DetailIntensity)+lerp((node_9564.rgb+node_7724.rgb),float3(0,0,1),_WaveIntensity)));
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
                float4 node_6560 = _Time;
                float2 node_3248 = (i.uv0+(node_6560.r*_RefractionSpeed)*float2(0.1,0.1));
                float3 _RefractionNormals_var = UnpackNormal(tex2D(_RefractionNormals,TRANSFORM_TEX(node_3248, _RefractionNormals)));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (_RefractionNormals_var.rgb.rg*_RefractionIntensity);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _GlossAmount;
                float perceptualRoughness = 1.0 - _GlossAmount;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = _SpecularColor.rgb;
                float specularMonochrome;
                float3 diffuseColor = _DiffuseColor.rgb; // Need this for specular when using metallic
                diffuseColor = EnergyConservationBetweenDiffuseAndSpecular(diffuseColor, specularColor, specularMonochrome);
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse * saturate((sceneZ-partZ)/_Depth) + specular;
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,saturate((sceneZ-partZ)/_Depth)),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform sampler2D _CameraDepthTexture;
            uniform float _GlossAmount;
            uniform float4 _SpecularColor;
            uniform sampler2D _WaveNormals; uniform float4 _WaveNormals_ST;
            uniform float _WaveScrollSpeed;
            uniform float _WaveIntensity;
            uniform sampler2D _DetailNormals; uniform float4 _DetailNormals_ST;
            uniform float _DetailScrollSpeed;
            uniform float _DetailIntensity;
            uniform float _MinuteDetailScrollSpeed;
            uniform sampler2D _MinuteDetailNormals; uniform float4 _MinuteDetailNormals_ST;
            uniform float _MinuteDetailIntensity;
            uniform float4 _DiffuseColor;
            uniform float _Depth;
            uniform float _RefractionSpeed;
            uniform sampler2D _RefractionNormals; uniform float4 _RefractionNormals_ST;
            uniform float _RefractionIntensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 projPos : TEXCOORD5;
                LIGHTING_COORDS(6,7)
                UNITY_FOG_COORDS(8)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 node_7107 = _Time;
                float node_8964 = (_MinuteDetailScrollSpeed*node_7107.r);
                float2 node_3213 = ((i.uv0+node_8964*float2(1,0))*1.2);
                float3 node_4502 = UnpackNormal(tex2D(_MinuteDetailNormals,TRANSFORM_TEX(node_3213, _MinuteDetailNormals)));
                float2 node_6353 = (i.uv0+node_8964*float2(0,1));
                float3 node_7637 = UnpackNormal(tex2D(_MinuteDetailNormals,TRANSFORM_TEX(node_6353, _MinuteDetailNormals)));
                float4 node_1568 = _Time;
                float node_9285 = (_DetailScrollSpeed*node_1568.r);
                float2 node_8611 = ((i.uv0+node_9285*float2(1,0))*1.2);
                float3 node_2498 = UnpackNormal(tex2D(_DetailNormals,TRANSFORM_TEX(node_8611, _DetailNormals)));
                float2 node_6125 = (i.uv0+node_9285*float2(0,1));
                float3 node_6823 = UnpackNormal(tex2D(_DetailNormals,TRANSFORM_TEX(node_6125, _DetailNormals)));
                float4 node_5188 = _Time;
                float node_6665 = (_WaveScrollSpeed*node_5188.r);
                float2 node_5163 = ((i.uv0+node_6665*float2(1,0))*1.2);
                float3 node_9564 = UnpackNormal(tex2D(_WaveNormals,TRANSFORM_TEX(node_5163, _WaveNormals)));
                float2 node_1597 = (i.uv0+node_6665*float2(0,1));
                float3 node_7724 = UnpackNormal(tex2D(_WaveNormals,TRANSFORM_TEX(node_1597, _WaveNormals)));
                float3 normalLocal = (lerp((node_4502.rgb+node_7637.rgb),float3(0,0,1),_MinuteDetailIntensity)+(lerp((node_2498.rgb+node_6823.rgb),float3(0,0,1),_DetailIntensity)+lerp((node_9564.rgb+node_7724.rgb),float3(0,0,1),_WaveIntensity)));
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
                float4 node_6560 = _Time;
                float2 node_3248 = (i.uv0+(node_6560.r*_RefractionSpeed)*float2(0.1,0.1));
                float3 _RefractionNormals_var = UnpackNormal(tex2D(_RefractionNormals,TRANSFORM_TEX(node_3248, _RefractionNormals)));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (_RefractionNormals_var.rgb.rg*_RefractionIntensity);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                UNITY_LIGHT_ATTENUATION(attenuation,i, i.posWorld.xyz);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _GlossAmount;
                float perceptualRoughness = 1.0 - _GlossAmount;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = _SpecularColor.rgb;
                float specularMonochrome;
                float3 diffuseColor = _DiffuseColor.rgb; // Need this for specular when using metallic
                diffuseColor = EnergyConservationBetweenDiffuseAndSpecular(diffuseColor, specularColor, specularMonochrome);
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse * saturate((sceneZ-partZ)/_Depth) + specular;
                fixed4 finalRGBA = fixed4(finalColor,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
