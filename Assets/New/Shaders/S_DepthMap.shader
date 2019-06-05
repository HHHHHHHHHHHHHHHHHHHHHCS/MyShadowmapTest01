Shader "HCS/S_DepthMap"
{
	Properties { }
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		
		Pass
		{
			CGPROGRAM
			
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _CameraDepthTexture;
			
			struct v2f
			{
				float4 pos: SV_POSITION;
				float4 projPos: TEXCOORD1; //screen pos
			};
			
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.projPos = ComputeScreenPos(o.pos);//计算到摄像机的投影位置
				return o;
			}
			
			float4 frag(v2f i): SV_TARGET
			{
				//深度图*摄像机投影位置=到深度
				float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r);
				return EncodeFloatRGBA(depth);
			}
			
			ENDCG
			
		}
	}
	FallBack "VertexLit"
}
