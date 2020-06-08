﻿Shader "Custom/Portal"
{
	 Properties
	 {
		 _TintColor("Tint Colour", Color) = (0.5, 0.5, 0.5, 1)
		 _CubMap("Cube Map", Cube) = "" {}
	 }

	Category
	{
		Tags{"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off

		SubShader
		{
			Stencil
			{
				Ref 2
				Comp Equal
				Pass Keep
				Fail Keep
			}

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag 
				#include "UnityCG.cginc"

				fixed4 _TintColor;
				samplerCUBE _CubMap;

				struct appdata_t
				{
					float4 vertex : POSITION;
				};

				struct v2f 
				{
					float4 vertex : POSITION;
					float3 viewDir : TEXCOORD1;
				};

				float4 _MainTex_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.viewDir = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;

					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
					float4 cubeTex = texCUBE(_CubMap, i.viewDir) * _TintColor;
					return cubeTex;
				}
				ENDCG
			}
		}
	}
}
