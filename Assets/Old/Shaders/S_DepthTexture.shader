﻿Shader "HCS/S_DepthTexture"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		
		LOD 100
		
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};
			
			struct v2f
			{
				float2 uv: TEXCOORD0;
				float4 vertex: SV_POSITION;
				float2 depth: TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.depth = o.vertex.zw;
				return o;
			}

			half4 frag(v2f i):SV_TARGET
			{
				//如果在顶点阶段进行计算可能出现错误
				//GPU会对片段找色器传入的参数进行插值计算
				float depth = i.depth.x/i.depth.y;
				half4 col = EncodeFloatRGBA(depth);
				return col;
			}
			
			ENDCG
			
		}
	}
}
