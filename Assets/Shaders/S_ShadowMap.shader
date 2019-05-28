Shader "HCS/S_ShadowMap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			// sampler2D unity_Lightmap;//若开启光照贴图,系统默认填入
			// flaot4 unity_LightmapST;//与上面同理
			
			struct v2f
			{
				float4 pos: SV_POSITION;
				float2 uv: TEXCOORD0;
				float2 uv2: TEXCOORD1;
				float4 proj: TEXCOORD2;
				float2 depth: TEXCOORD4;
			};
			
			float4x4 proMatrix;
			sampler2D depthTexture;
			
			v2f vert(appdata_full v)
			{
				v2f o;
				
				o.pos = UnityObjectToClipPos(v.vertex);
				
				o.depth = o.pos.zw;
				proMatrix = mul(proMatrix, unity_ObjectToWorld);
				o.proj = mul(proMatrix, v.vertex);
				
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv2 = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				
				return o;
			}
			
			half4 frag(v2f v): SV_TARGET
			{
				float3 lightmapColor = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, v.uv2));
				half4 col = tex2D(_MainTex, v.uv);
				
				col.rgb *= lightmapColor;
				
				float depth = v.depth.x / v.depth.y;
				half4 dcol = tex2Dproj(depthTexture, v.proj);
				float d = DecodeFloatRGBA(dcol);
				float shadowScale = 1;
				
				if (depth > d)
				{
					shadowScale = 0.55;
				}
				return col*shadowScale;
			}
			
			ENDCG
			
		}
	}
}
