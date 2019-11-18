Shader "Custom/GhostShader"
{
    Properties
    {
		[Header(Basics)]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[Toggle(NORMALS)] _NORMALS("Use Normal Map", Float) = 0
		_Normal("Normal Map", 2D) = "bump" {}

		[Header(Extra Emission Texture)]
		_Mask("Emission Mask", 2D) = "black" {}
		_EMIntensity("Emission Mask (EM) Intensity", Range(0, 8.0)) = 3.0
		_RimColorMask("EM Rim Color", Color) = (0, 0.2, 0.15, 0.0)

		[Header(Noise)]
		_NoiseTEX("Noise Texture", 2D) = "white" {}
		_NScale("Noise Scale", Range(0,5)) = 1
		_NSpeed("Noise Speed", Range(-5, 5)) = 1

		[Header(Base Rim)]
		_RimColor("Rim Color", Color) = (0, 0.8, 0.8, 0.0)
		_RimPower("Rim Power", Range(0.5, 6.0)) = 2.0
		_GlowBrightness("Glow Brightness", Range(0.01, 20.0)) = 3.0

		[Header(Fade Effect)]
		[Toggle(FADE)] _FADE("Fade To Bottom", Float) = 1
		[Toggle(INVERT)] _INVERT("Invert Fade", Float) = 0
		_FadeOffset("Fade Offset", Range(-10, 10.0)) = 1

		[Header(Cutoff)]
		[Toggle(HARD)] _HARD("Cutoff Rim", Float) = 0
		_Cutoff("Cutoff", Range(0, 1)) = 0.5
		_Smooth("Smoothness", Range(-1,1)) = 0.1

		[Header(Blending)]
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcFactor("Source Factor", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstFactor("Destination Factor", Int) = 1
    }
    SubShader
	{
			Tags{"Queue" = "Transparent"}
			LOD 200

			//Blending based on enums in Properties
			Blend[_SrcFactor][_DstFactor]

			Pass{

				Name "Empty"
				ZWrite On 
				ColorMask 0
			}

			CGPROGRAM

			//Physically based standard lightint model, but enables shadows on all light types and is toon
			#pragma surface surf ToonRamp keepalpha vertex:vert

			//Uses shader model 3.0 for better lighting quality
#pragma target 3.0
#pragma shader_feature FADE
#pragma shader_feature NORMALS
#pragma shader_feature INVERT
#pragma shader_feature HARD

			inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten) {

#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(lightDir);
#endif

			float d = dot(s.Normal, lightDir);
			float dChange = fwidth(d);
			float3 lightIntensity = smoothstep(0, dChange + 0.05, d);

			half4 c;
			atten *= smoothstep(fwidth(d), d, 0.5);
			c.rgb = s.Albedo * _LightColor0.rgb * lightIntensity * (atten * 2);
			c.a = s.Alpha;

			return c;
			}

		struct Input {
			float2 uv_MainTex;
			float2 uv2_Noise;
			float2 uv_Normal;
			float3 viewDir;
			float4 objPos;

		};

		//Vert shader
		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o); //Initialize o to be of struct input
			o.objPos = mul(unity_ObjectToWorld, v.vertex.xyz); //Translate from world space to screen space
		}


		float4 _Color, _RimColor, _RimColorMask;
		float _RimPower, _NScale;
		sampler2D _MainTex, _NoiseTEX, _Mask, _Normal;
		float _FadeOffset, _NSpeed, _GlowBrightness, _EMIntensity;
		float _Cutoff, _Smooth;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutput o)
		{
			//Albedo comes from texture + tinted Color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			o.Albedo = c.rgb;

			//red mask for extra Emission
			fixed4 mask = tex2D(_Mask, IN.uv_MainTex);

			//noise that scales and moves over time
			float NscrollSpeed = _Time.x * _NSpeed;
			fixed4 n = tex2D(_NoiseTEX, IN.uv2_Noise * _NScale + NscrollSpeed);

			//Same noise, but bigger
			fixed4 n2 = tex2D(_NoiseTEX, IN.uv2_Noise * (_NScale*0.5) + NscrollSpeed);

			//Same but smaller
			fixed4 n3 = tex2D(_NoiseTEX, IN.uv2_Noise * (_NScale*2) + NscrollSpeed);

			//combined noise
			float combinedNoise = (n.r * n2.r * 2) * n3.r * 2; 

#if NORMALS
			o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
#endif

			//Rim lighting with noise
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir + (combinedNoise * 2)), o.Normal));
			rim = pow(rim, _RimPower);

#if HARD
			rim = smoothstep(_Cutoff, _Cutoff + _Smooth, rim);
#endif

			//add Color
			float3 coloredRim = rim * _RimColor;

			//colored emission mask
			float3 coloredEMask = _RimColorMask * mask.r * _EMIntensity;

			//combined
			o.Emission = coloredRim + coloredEMask;

			//create gradient fade over obj POSITION
			float fade = saturate(IN.objPos.y + _FadeOffset);

#if INVERT
			fade = 1 - fade;
#endif

#if FADE

			//fade rim over obj
			rim *= fade;

			//add fade for a bit more strength at the end
			rim += fade / 10;

			//add Color
			float3 coloredRimFade = pow(rim, _RimPower) * rim * _RimColor;

			//colored emission mask
			float3 coloredEMaskFade = _RimColorMask * mask.r * fade * _EMIntensity;

			//combined
			o.Emission = coloredRimFade + coloredEMaskFade;
#endif

			//Add extra glow

			o.Emission *= _GlowBrightness;

		    }

		ENDCG
	}
		FallBack "Diffuse"
}
