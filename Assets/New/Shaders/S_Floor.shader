Shader "HCS/S_Floor"
{
	Properties
	{
		_ShadowMap ("ShadowMap", 2D) = "white"
	}
	
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			//材质反射属性
			static float4 materialDiffuse = {
				1.0, 0.0, 0.0, 1.0
			};
			
			static float4 materialAmbient = {
				1.0, 0.0, 0.0, 1.0
			};
			
			//灯光属性
			static float4 lightDiffuse = {
				0.2, 0.2, 0.2, 1.0
			};
			static float4 lightAmbient = {
				0.2, 0.2, 0.2, 1.0
			};
			
			static float3 lightPos = {
				0.0, 5.0, 0.0
			};
			static float3 lightDir = {
				0.0, 1.0, 0.0
			};
			
			//摄像机属性
			sampler2D _ShadowMap;
			float4x4  _ProjMatrix;
			static float _TexSize = 1024;
			static float _Bias = 0.012;
			static float _FarClip = 10;
			static float _NearClip = 0.3;
			
			struct a2v
			{
				float3 pos: POSITION0;
				float3 norm: NORMAL0;
				float2 tex: TEXCOORD0;
			};
			
			struct v2f
			{
				float4 pos: SV_POSITION;
				float4 col: COLOR0;
				float4 projTex: TEXCOORD1;
			};
			
			v2f vert(a2v v)
			{
				v2f o;
				
				o.pos = UnityObjectToClipPos(float4(v.pos, 1.0f));
				
				float3 normal = normalize(UnityObjectToWorldNormal(v.norm.xyz));
				
				float diffComp = max(dot(lightDir, normal), 0.0f);
				
				float3 diffuse = diffComp * (lightDiffuse * materialDiffuse).rgb;
				float3 ambient = lightAmbient * materialAmbient;
				
				o.col = float4(diffuse + ambient, materialAmbient.a);
				
				float4 posWorld = mul(unity_ObjectToWorld, float4(v.pos, 1.0f));
				
				o.projTex = mul(_ProjMatrix, posWorld);
				
				return o;
			}
			
			float4 frag(v2f v): SV_TARGET
			{
				v.projTex.xy /= v.projTex.w;
				v.projTex.x = 0.5 * v.projTex.x + 0.5f;
				v.projTex.y = 0.5 * v.projTex.y + 0.5f;
				
				float depth = v.projTex.z / v.projTex.w;
				
				//切面计算
				float sceneDepth = _NearClip * (depth + 1.0) / (_FarClip + _NearClip - depth * (_FarClip - _NearClip));
				
				float2 texelpos = _TexSize * v.projTex.xy;
				
				
				//软边阴影
				float2 lerps = frac(texelpos);
				float dx = 1.0f / _TexSize;
				float s0 = (DecodeFloatRGBA(tex2D(_ShadowMap, v.projTex.xy)) + _Bias < sceneDepth) ? 0.0f: 1.0f;
				float s1 = (DecodeFloatRGBA(tex2D(_ShadowMap, v.projTex.xy + float2(dx, 0.0f))) + _Bias < sceneDepth) ? 0.0f: 1.0f;
				float s2 = (DecodeFloatRGBA(tex2D(_ShadowMap, v.projTex.xy + float2(0.0f, dx))) + _Bias < sceneDepth) ? 0.0f: 1.0f;
				float s3 = (DecodeFloatRGBA(tex2D(_ShadowMap, v.projTex.xy + float2(dx, dx))) + _Bias < sceneDepth) ? 0.0f: 1.0f;
				
				float shadowCoeff = lerp(lerp(s0, s1, lerps.x), lerp(s2, s3, lerps.x), lerps.y);
				
				return float4(shadowCoeff * v.col.rgb, materialDiffuse.a);
			}
			
			ENDCG
			
		}
	}
}
