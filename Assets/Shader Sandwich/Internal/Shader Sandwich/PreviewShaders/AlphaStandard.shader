Shader "Hidden/ShaderSandwich/Alpha Standard" {
Properties {_MainTex ("Texture to blend", 2D) = "black" {} 
_Color ("Main Color", Color) = (1,1,1,1) }
SubShader {
	Tags { "Queue" = "Transparent" }
	Pass {
		Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ColorMask RGBA Fog {Mode Off}

		Lighting Off
		SetTexture [_MainTex] {constantColor [_Color] combine texture*constant, texture*constant}
	}
}
}