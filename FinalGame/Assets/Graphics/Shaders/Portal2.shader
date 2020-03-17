﻿Shader "Custom/Portal2"
{
	Properties
{
	_Color("Tint", Color) = (1, 1, 1, .5)
	_FoamC("Foam", Color) = (1, 1, 1, .5)
	_MainTex("Main Texture", 2D) = "white" {}
	_TextureDistort("Texture Wobble", range(0,1)) = 0.1
	_NoiseTex("Extra Wave Noise", 2D) = "white" {}
	_Speed("Wave Speed", Range(0,1)) = 0.5
	_Amount("Wave Amount", Range(0,1)) = 0.6
	_Scale("Scale", Range(0,1)) = 0.5
	_Height("Wave Height", Range(0,1)) = 0.1
	_Foam("Foamline Thickness", Range(0,10)) = 8
	 _MaskInt("RenderTexture Mask", 2D) = "white" {}

	[Header(Caustics)]
		_CausticsTex("Caustics Texture", 2D) = "white"{}

		//Tiling x and y, offset x and y
		_Caustics_ST("Caustics Tiling", Vector) = (1,1,0,0)
		_Caustics2_ST("Caustics 2 Tiling", Vector) = (1,1,0,0)

		_CausticsSpeed("Caustics Speed", Vector) = (1,1,0,0)
		_Caustics2Speed("Caustics 2 Speed", Vector) = (1,1,0,0)

		_SplitRGB("Split RGB Intensity", Range(0,1)) = 0.5
}
SubShader
{
	Tags { "RenderType" = "Opaque"  "Queue" = "Transparent" }
	LOD 100
	Blend OneMinusDstColor One
	Cull Off

	GrabPass{
		Name "BASE"
		Tags{ "LightMode" = "Always" }
			}
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
			float2 uv : TEXCOORD3;
			UNITY_FOG_COORDS(1)
			float4 vertex : SV_POSITION;
			float4 scrPos : TEXCOORD2;//
			float4 worldPos : TEXCOORD4;//
		};
		float _TextureDistort;
		float4 _Color;
		sampler2D _CameraDepthTexture; //Depth Texture
		sampler2D _MainTex, _NoiseTex;//Textures used for the water
		sampler2D _CausticsTex;
		float4 _MainTex_ST;
		float _Speed, _Amount, _Height, _Foam, _Scale;//Different parameters to manipulate the water effect
		float4 _FoamC;
				sampler2D _MaskInt;

		uniform float3 _Position;
		uniform sampler2D _GlobalEffectRT;
		uniform float _OrtographicCamSize;

		float4 _Caustics_ST;
		float4 _Caustics2_ST;

		float4 _CausticsSpeed;
		float4 _Caustics2Speed;

		half _SplitRGB;

		v2f vert(appdata v)
		{
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);
			float4 tex = tex2Dlod(_NoiseTex, float4(v.uv.xy, 0, 0));//extra noise tex
			v.vertex.y += sin(_Time.z * _Speed + (v.vertex.x * v.vertex.z * _Amount * tex)) * _Height;//movement
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);

			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.scrPos = ComputeScreenPos(o.vertex);
			UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
		}

		float3 causticsSample(sampler2D _CausticsTex, float2 UV, float4 _Caustics_ST, float4 _CausticsSpeed)
		{
			//Caustics sampling uv
			fixed2 uv = UV * _Caustics_ST.xy + _Caustics_ST.zw;

			//animates UVs over time
			uv += _CausticsSpeed * _Time.y;

			// RGB split
			fixed s = _SplitRGB;
			fixed r = tex2D(_CausticsTex, uv + fixed2(+s, +s)).r;
			fixed g = tex2D(_CausticsTex, uv + fixed2(+s, -s)).g;
			fixed b = tex2D(_CausticsTex, uv + fixed2(-s, -s)).b;

			float3 caustics = fixed3(r, g, b);

			return caustics;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			//Render texture UV
			float2 uv = i.worldPos.xz - _Position.xz;
			uv = uv / (_OrtographicCamSize * 2);
			uv += 0.5;

			fixed3 c1 = causticsSample(_CausticsTex, i.uv, _Caustics_ST, _CausticsSpeed);
			fixed3 c2 = causticsSample(_CausticsTex, i.uv, _Caustics2_ST, _Caustics2Speed);

			//Ripples
			float ripples = tex2D(_GlobalEffectRT, uv).b;

			// mask to prevent bleeding
		   float4 mask = tex2D(_MaskInt, uv);
		   ripples *= mask.a;

		   // sample the texture
		   fixed distortx = tex2D(_NoiseTex, (i.worldPos.xz * _Scale) + (_Time.x * 2)).r;// distortion alpha
			 distortx += (ripples * 2);

		   half4 col = tex2D(_MainTex, (i.worldPos.xz * _Scale) - (distortx * _TextureDistort));// texture times tint;        
		   half depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos))); // depth
		   half4 foamLine = 1 - saturate(_Foam * (depth - i.scrPos.w));// foam line by comparing depth and screenposition

		   half3 caustics = min(c1, c2);
		   col *= _Color;
		   col += (step(0.4 * distortx,foamLine) * _FoamC); // add the foam line and tint to the texture
		   col += half4(caustics.r, caustics.g, caustics.b, col.a);
		   col = saturate(col) * col.a;


			ripples = step(0.99, ripples * 3);
		  float4 ripplesColored = ripples * _FoamC;

		  return   saturate(col + ripplesColored);
	   }
	   ENDCG
   }
}
}