Shader "Hidden/ShaderSandwich/Multiply" {
Properties {_MainTex ("Texture to blend", 2D) = "black"{} 
_Color ("Main Color", Color) = (1,1,1,1) }
SubShader {
	Tags { "Queue" = "Transparent" }
	Pass {
		Blend DstColor Zero ZWrite Off ColorMask RGB Fog {Mode Off}
		Lighting Off
		SetTexture [_MainTex] { combine texture}
		SetTexture [_MainTex] {constantColor [_Color] combine previous+constant alpha}
	}
}
}