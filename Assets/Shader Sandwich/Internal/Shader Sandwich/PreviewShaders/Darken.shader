Shader "Hidden/ShaderSandwich/Darken" {
Properties {_MainTex ("Texture to blend", 2D) = "black"{} 
_Color ("Main Color", Color) = (1,1,1,1) }
SubShader {
	Tags { "Queue" = "Transparent" }
	Pass {
		Blend One One BlendOp Min ZWrite Off ColorMask RGBA Fog {Mode Off}
		Lighting Off
		SetTexture [_MainTex] {constantColor (0,0,0,1) combine texture, constant}
		SetTexture [_MainTex] {constantColor [_Color] combine previous*constant alpha}
	}
}
}