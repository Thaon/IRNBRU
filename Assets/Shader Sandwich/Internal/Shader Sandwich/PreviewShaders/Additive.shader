﻿Shader "Hidden/ShaderSandwich/Additive" {
Properties {_MainTex ("Texture to blend", 2D) = "black"{} 
_Color ("Main Color", Color) = (1,1,1,1) }
SubShader {
	Tags { "Queue" = "Transparent" }
	Pass {
		Blend One One ZWrite Off ColorMask RGBA Fog {Mode Off}
		Lighting Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _Color;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv)*_Color.a;
				//col.rgb *= col.a;
				return col;
			}
			ENDCG
		//SetTexture [_MainTex] {constantColor [_Color] combine texture*constant alpha}
		//SetTexture [_MainTex] {constantColor [_Color] combine previous*constant}
	}
}
}