Shader "Custom/CausticsDistortion"
{
    Properties
    {
        [HDR] _TintColor("Tint Colour", Color) = (1,1,1,1)
        _MainTex("Main Texture", 2D) = "black" {}
        _BumpMap("Normal Map", 2D) = "bump"{}
        _BumpAmount("Distortion", Float) = 10
        _InvFade("Soft Particles Factor", Range(0,10)) = 1.0
    }

    Category
    {
        Tags{"Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        Fog {Mode Off}
        
        SubShader
        {
            GrabPass
            {
                // "_GrabTexture"							
                // Name "BASE"
                // Tags { "LightMode" = "Always" }
            }

            Pass
            {

                // Name "BASE"
                // Tags { "LightMode" = "Always" }
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #pragma multi_compile_particles
                #include "UnityCG.cginc"

                //defines how to treat the vertex data
                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    fixed4 color : COLOR;
                };

                //define how to treat the data for the fragment shader
                struct v2f
                {
                    float4 vertex : POSITION;
                    float4 uvgrab : TEXCOORD0;
                    float2 uvbump : TEXCOORD1;
                    float2 uvmain : TEXCOORD2;
                    fixed4 color : COLOR;

                    //#ifdef SOFTPARTICLES_ON
                    float4 projPos : TEXCOORD3;
                    //#endif
                };

                sampler2D _MainTex;
                sampler2D _BumpMap;

                float _BumpAmount;
                sampler2D _GrabTexture; //
                float4 _GrabTexture_TexelSize;
                fixed4 _TintColor;

                float4 _BumpMap_ST; //Grabs UV and Offset values of Normal Map
                float4 _MainTex_ST; //Grabs UV and Offset values of MainTex

                //vertex shader
                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex); //from model coordinates to world coordinates

                    //#ifdef SOFTPARTICLES_ON
                    o.projPos = ComputeScreenPos (o.vertex);
                    COMPUTE_EYEDEPTH(o.projPos.z);
                    //#endif

                    o.color = v.color;

                    #if UNITY_UV_STARTS_AT_TOP
                        float scale = -1.0;
                    #else
                        float scale = 1.0;
                    #endif

	                o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
	                o.uvgrab.zw = o.vertex.w;
                    #if UNITY_SINGLE_PASS_STEREO
	                o.uvgrab.xy = TransformStereoScreenSpaceTex(o.uvgrab.xy, o.uvgrab.w);
                    #endif

                    //Calculate the distance between the object and the camera position
                    o.uvgrab.z /= distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex))/10;

                    o.uvbump = TRANSFORM_TEX(v.texcoord, _BumpMap);
                    o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);

                    return o;

                }

                sampler2D _CameraDepthTexture; //samples the camera depth into a texture
                float _InvFade;

                //fragment shader
                half4 frag(v2f i) : COLOR
                {
                    float sceneZ = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
                    float partZ = i.projPos.z;
                    float fade = 1-saturate(_InvFade * (sceneZ - partZ));
                    if (_InvFade < 0.001) fade = 0;

                    half2 bump = UnpackNormal(tex2D( _BumpMap, i.uvbump )).rg;
                    float2 offset = bump * _BumpAmount * _GrabTexture_TexelSize.xy;
                    i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;

                    //unpacks the texture, multiples it by the tint color and feeds the result to the emission channel
                    half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
                    fixed4 tex = tex2D(_MainTex, i.uvmain) * i.color;
                    fixed4 emission = col * i.color + tex * _TintColor + fade * _TintColor;
                    emission.a = _TintColor.a * i.color.a;
                    return emission;
                }
                ENDCG

            }
        }
    }
}

